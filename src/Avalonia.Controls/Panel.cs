// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Specialized;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Avalonia.Controls
{
    /// <summary>
    /// Base class for controls that can contain multiple children.
    /// </summary>
    /// <remarks>
    /// Controls can be added to a <see cref="Panel"/> by adding them to its <see cref="Children"/>
    /// collection. All children are layed out to fill the panel.
    /// </remarks>
    public class Panel : Control, IPanel
    {
        /// <summary>
        /// Defines the <see cref="Background"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BackgroundProperty =
            Border.BackgroundProperty.AddOwner<Panel>();

        /// <summary>
        /// Initializes static members of the <see cref="Panel"/> class.
        /// </summary>
        static Panel()
        {
            AffectsRender<Panel>(BackgroundProperty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        public Panel()
        {
            Children.CollectionChanged += ChildrenChanged;
        }

        /// <summary>
        /// Gets the children of the <see cref="Panel"/>.
        /// </summary>
        [Content]
        public Controls Children { get; } = new Controls();

        /// <summary>
        /// Gets or Sets Panel background brush.
        /// </summary>
        public IBrush Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        /// <summary>
        /// Renders the visual to a <see cref="DrawingContext"/>.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        public override void Render(DrawingContext context)
        {
            var background = Background;
            if (background != null)
            {
                var renderSize = Bounds.Size;
                context.FillRectangle(background, new Rect(renderSize));
            }

            base.Render(context);
        }

        /// <summary>
        /// Marks a property on a child as affecting the parent panel's arrangement.
        /// </summary>
        /// <param name="properties">The properties.</param>
        protected static void AffectsParentArrange<TPanel>(params AvaloniaProperty[] properties)
            where TPanel : class, IPanel
        {
            foreach (var property in properties)
            {
                property.Changed.Subscribe(AffectsParentArrangeInvalidate<TPanel>);
            }
        }

        /// <summary>
        /// Marks a property on a child as affecting the parent panel's measurement.
        /// </summary>
        /// <param name="properties">The properties.</param>
        protected static void AffectsParentMeasure<TPanel>(params AvaloniaProperty[] properties)
            where TPanel : class, IPanel
        {
            foreach (var property in properties)
            {
                property.Changed.Subscribe(AffectsParentMeasureInvalidate<TPanel>);
            }
        }

        /// <summary>
        /// Called when the <see cref="Children"/> collection changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems.Count == 1)
                    {
                        IControl control = (IControl)e.NewItems[0];

                        LogicalChildren.Insert(e.NewStartingIndex, control);
                        VisualChildren.Insert(e.NewStartingIndex, control);
                    }
                    else
                    {
                        var items = e.NewItems;
                        var controls = new IControl[items.Count];

                        items.CopyTo(controls, 0);

                        LogicalChildren.InsertRange(e.NewStartingIndex, controls);
                        VisualChildren.InsertRange(e.NewStartingIndex, controls);
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Move:
                {
                    LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems.Count, e.NewStartingIndex);
                    VisualChildren.MoveRange(e.OldStartingIndex, e.OldItems.Count, e.NewStartingIndex);
                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems.Count == 1)
                    {
                        IControl control = (IControl)e.OldItems[0];

                        LogicalChildren.Remove(control);
                        VisualChildren.Remove(control);
                    }
                    else
                    {
                        var items = e.OldItems;
                        var controls = new IControl[items.Count];

                        items.CopyTo(controls, 0);

                        LogicalChildren.RemoveAll(controls);
                        VisualChildren.RemoveAll(controls);
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Replace:
                {
                    for (var i = 0; i < e.OldItems.Count; ++i)
                    {
                        var index = i + e.OldStartingIndex;
                        var child = (IControl)e.NewItems[i];
                        LogicalChildren[index] = child;
                        VisualChildren[index] = child;
                    }
                    break;
                }

                case NotifyCollectionChangedAction.Reset:
                {
                    throw new NotSupportedException();
                }
            }

            InvalidateMeasure();
        }

        private static void AffectsParentArrangeInvalidate<TPanel>(AvaloniaPropertyChangedEventArgs e)
            where TPanel : class, IPanel
        {
            var control = e.Sender as IControl;
            var panel = control?.VisualParent as TPanel;
            panel?.InvalidateArrange();
        }

        private static void AffectsParentMeasureInvalidate<TPanel>(AvaloniaPropertyChangedEventArgs e)
            where TPanel : class, IPanel
        {
            var control = e.Sender as IControl;
            var panel = control?.VisualParent as TPanel;
            panel?.InvalidateMeasure();
        }
    }
}
