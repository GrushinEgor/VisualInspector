﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace VisualInspector.Infrastructure
{
    public class EventVisualFactory : IVisualFactory<EventViewModel>
    {
        private readonly IDictionary<string, Pen> pens;
        private readonly IDictionary<string, Brush> brushes;

        public EventVisualFactory(IDictionary<string, Pen> pens, IDictionary<string, Brush> brushes)
        {
            this.pens = pens;
            this.brushes = brushes;
        }


        public DrawingVisual Create(EventViewModel viewModel, DrawingVisual canvas, Rect rect)
        {
            using (var context = canvas.RenderOpen())
            {
                Brush brush = brushes[viewModel.GetWarningLevel().ToString()];
                
                context.DrawRectangle(Brushes.Black, null, rect);
                var rect2 = new Rect(new Point(rect.Location.X + 1, rect.Location.Y + 1),
                    new Size(rect.Size.Width - 2, rect.Size.Height - 2));
                context.DrawRectangle(brush, null, rect2);
                
            }
            return canvas;
        }


        public DrawingVisual Change(EventViewModel viewModel, DrawingVisual oldVisual, Rect rect)
        {
            using (var context = oldVisual.RenderOpen())
            {
                context.DrawRectangle(Brushes.Black, null, rect);
                var rect2 = new Rect(new Point(rect.Location.X + 1, rect.Location.Y + 1),
                    new Size(rect.Size.Width - 2, rect.Size.Height - 2));
                context.DrawRectangle(Brushes.Blue, null, rect2);
            }
            return oldVisual;
        }
    }
}
