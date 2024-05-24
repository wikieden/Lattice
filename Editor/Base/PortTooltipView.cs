﻿using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lattice.Editor.Views
{
    public sealed partial class PortView
    {
        /// <summary>A specific implementation of tooltips for <see cref="PortView"/>. Supports complex tooltips on connected edges.</summary>
        private sealed class PortTooltipView : GraphTooltipView
        {
            /// <summary>The origin port directly associated with this tooltip.</summary>
            private readonly PortView port;

            /// <summary>Fake-looking nodes that appear when hovering a port with a hidden edge.</summary>
            private List<FakeNodeView> fakeNodeViews;

            private LatticeGraphView parentGraph;

            /// <inheritdoc />
            public PortTooltipView(IHasGraphTooltip source) : base(source)
            {
                port = (PortView)source;

                // Clean up any fake nodes we created for this view when this element is completely removed.
                RegisterCallbackOnce<DetachFromPanelEvent, PortTooltipView>(static (_, args) => args.RemoveFakeNodeViews(), this);
            }

            private void RemoveFakeNodeViews()
            {
                if (fakeNodeViews == null)
                {
                    return;
                }

                foreach (FakeNodeView nodeView in fakeNodeViews)
                {
                    parentGraph.RemoveElement(nodeView);
                }
                fakeNodeViews.Clear();
            }

            /// <inheritdoc />
            public override void UpdateTooltip(GraphTooltipEventSource evtSource)
            {
                base.UpdateTooltip(evtSource);
                
                // Handle connected edges tooltips
                switch (evtSource)
                {
                    case GraphTooltipEventSource.PointerEvent:
                        // If the pointer created this view, show tooltips for ports connected via edges.
                        ShowTooltipsOnConnectedEdges();
                        break;
                    case GraphTooltipEventSource.ForceShow:
                        // If the tooltip was force-enabled, only perform the additional logic below.
                        break;
                    default:
                    case GraphTooltipEventSource.ShowGraphTooltipCall:
                        // If the show event was sent manually, don't perform any extra logic.
                        return;
                }

                // Forcibly display any hidden edges by setting a USS class.
                foreach (EdgeView edge in port.Edges)
                {
                    if (edge.SerializedEdge?.IsHidden ?? false)
                    {
                        edge.AddToClassList(EdgeView.ForcedHoverUssClassName);
                    }
                }
            }

            private void ShowTooltipsOnConnectedEdges()
            {
                parentGraph ??= port.GetFirstAncestorOfType<LatticeGraphView>();
                Rect rect = parentGraph.contentRect;
                foreach (EdgeView edge in port.Edges)
                {
                    bool isEdgeHidden = edge.SerializedEdge?.IsHidden ?? false;
                    ShowTooltip((PortView)(edge.input == port ? edge.output : edge.input), isEdgeHidden);
                }
                
                // Ensure this tooltip is always in front.
                BringToFront();

                return;

                void ShowTooltip(PortView otherPort, bool isEdgeHidden)
                {
                    if (otherPort == null)
                    {
                        return;
                    }

                    if (!isEdgeHidden || IsPortVisible())
                    {
                        // Show a normal tooltip if the edge isn't marked as hidden, or the port is visible.
                        parentGraph.ShowGraphTooltip(otherPort);
                    }
                    else
                    {
                        // Show a 'fake' tooltip if it's not, and align it with the edge connection.
                        fakeNodeViews ??= new List<FakeNodeView>();
                        FakeNodeView fakeNodeView = new(otherPort);
                        fakeNodeViews.Add(fakeNodeView);
                        parentGraph.AddElement(fakeNodeView);
                        fakeNodeView.PositionOnScreenAlignedTowardsPort(port, true);
                    }

                    return;

                    bool IsPortVisible()
                    {
                        // Check whether the other port is visible on the screen.
                        Rect otherLocalBound = otherPort.localBound;
                        return rect.Contains(
                            otherPort.ChangeCoordinatesTo(parentGraph, new Vector2(otherLocalBound.width / 2f, otherLocalBound.height / 2f))
                        );
                    }
                }
            }

            /// <inheritdoc />
            protected override void OnHide()
            {
                parentGraph ??= port.GetFirstAncestorOfType<LatticeGraphView>();
                
                // Hide any FakeNodeView tooltips we created.
                RemoveFakeNodeViews();

                // Hide any tooltips from connected ports.
                foreach (EdgeView edge in port.Edges)
                {
                    HideTooltip(edge.input == port ? edge.output : edge.input);
                }

                // Hide any hidden edges by resetting the USS class.
                foreach (EdgeView edge in port.Edges)
                {
                    if (edge.SerializedEdge?.IsHidden ?? false)
                    {
                        edge.RemoveFromClassList(EdgeView.ForcedHoverUssClassName);
                    }
                }

                return;

                void HideTooltip(Port otherPort)
                {
                    if (otherPort == null)
                    {
                        return;
                    }

                    parentGraph.HideGraphTooltip(otherPort);
                }
            }
        }
    }
}
