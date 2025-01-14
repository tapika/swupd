//   OData .NET Libraries ver. 5.6.3
//   Copyright (c) Microsoft Corporation
//   All rights reserved. 
//   MIT License
//   Permission is hereby granted, free of charge, to any person obtaining a copy of
//   this software and associated documentation files (the "Software"), to deal in
//   the Software without restriction, including without limitation the rights to use,
//   copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
//   Software, and to permit persons to whom the Software is furnished to do so,
//   subject to the following conditions:

//   The above copyright notice and this permission notice shall be included in all
//   copies or substantial portions of the Software.

//   THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//   FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//   COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//   IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//   CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace Microsoft.Data.Spatial
{
    using System.Spatial;

    /// <summary>
    /// Base class to create a unified set of handlers for Geometry and Geography
    /// </summary>
    internal abstract class DrawBoth
    {
        /// <summary>
        /// Gets the draw geography.
        /// </summary>
        public virtual GeographyPipeline GeographyPipeline
        {
            get { return new DrawGeographyInput(this); }
        }

        /// <summary>
        /// Gets the draw geometry.
        /// </summary>
        public virtual GeometryPipeline GeometryPipeline
        {
            get { return new DrawGeometryInput(this); }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="DrawBoth"/> to <see cref="SpatialPipeline"/>.
        /// </summary>
        /// <param name="both">The instance to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator SpatialPipeline(DrawBoth both)
        {
            if (both != null)
            {
                return new SpatialPipeline(both.GeographyPipeline, both.GeometryPipeline);
            }

            return null;
        }

        /// <summary>
        /// Draw a point in the specified coordinate
        /// </summary>
        /// <param name="position">Next position</param>
        /// <returns>The position to be passed down the pipeline</returns>
        protected virtual GeographyPosition OnLineTo(GeographyPosition position)
        {
            return position;
        }

        /// <summary>
        /// Draw a point in the specified coordinate
        /// </summary>
        /// <param name="position">Next position</param>
        /// <returns>The position to be passed down the pipeline</returns>
        protected virtual GeometryPosition OnLineTo(GeometryPosition position)
        {
            return position;
        }

        /// <summary>
        /// Begin drawing a figure
        /// </summary>
        /// <param name="position">Next position</param>
        /// <returns>The position to be passed down the pipeline</returns>
        protected virtual GeographyPosition OnBeginFigure(GeographyPosition position)
        {
            return position;
        }

        /// <summary>
        /// Begin drawing a figure
        /// </summary>
        /// <param name="position">Next position</param>
        /// <returns>The position to be passed down the pipeline</returns>
        protected virtual GeometryPosition OnBeginFigure(GeometryPosition position)
        {
            return position;
        }

        /// <summary>
        /// Begin drawing a spatial object
        /// </summary>
        /// <param name="type">The spatial type of the object</param>
        /// <returns>The type to be passed down the pipeline</returns>
        protected virtual SpatialType OnBeginGeography(SpatialType type)
        {
            return type;
        }

        /// <summary>
        /// Begin drawing a spatial object
        /// </summary>
        /// <param name="type">The spatial type of the object</param>
        /// <returns>The type to be passed down the pipeline</returns>
        protected virtual SpatialType OnBeginGeometry(SpatialType type)
        {
            return type;
        }

        /// <summary>
        /// Ends the current figure
        /// </summary>
        protected virtual void OnEndFigure()
        {
        }

        /// <summary>
        /// Ends the current spatial object
        /// </summary>
        protected virtual void OnEndGeography()
        {
        }

        /// <summary>
        /// Ends the current spatial object
        /// </summary>
        protected virtual void OnEndGeometry()
        {
        }

        /// <summary>
        /// Setup the pipeline for reuse
        /// </summary>
        protected virtual void OnReset()
        {
        }

        /// <summary>
        /// Set the coordinate system
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>the coordinate system to be passed down the pipeline</returns>
        protected virtual CoordinateSystem OnSetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            return coordinateSystem;
        }

        /// <summary>
        /// This class is responsible for taking the calls to DrawGeography and delegating them to the unified
        /// handlers
        /// </summary>
        private class DrawGeographyInput : GeographyPipeline
        {
            /// <summary>
            /// the DrawBoth instance that should be delegated to
            /// </summary>
            private readonly DrawBoth both;

            /// <summary>
            /// Initializes a new instance of the <see cref="DrawGeographyInput"/> class.
            /// </summary>
            /// <param name="both">The both.</param>
            public DrawGeographyInput(DrawBoth both)
            {
                this.both = both;
            }

            /// <summary>
            /// Draw a point in the specified coordinate
            /// </summary>
            /// <param name="position">Next position</param>
            public override void LineTo(GeographyPosition position)
            {
                both.OnLineTo(position);
            }

            /// <summary>
            /// Begin drawing a figure
            /// </summary>
            /// <param name="position">Next position</param>
            public override void BeginFigure(GeographyPosition position)
            {
                both.OnBeginFigure(position);
            }

            /// <summary>
            /// Begin drawing a spatial object
            /// </summary>
            /// <param name="type">The spatial type of the object</param>
            public override void BeginGeography(SpatialType type)
            {
                both.OnBeginGeography(type);
            }

            /// <summary>
            /// Ends the current figure
            /// </summary>
            public override void EndFigure()
            {
                both.OnEndFigure();
            }

            /// <summary>
            /// Ends the current spatial object
            /// </summary>
            public override void EndGeography()
            {
                both.OnEndGeography();
            }

            /// <summary>
            /// Setup the pipeline for reuse
            /// </summary>
            public override void Reset()
            {
                both.OnReset();
            }

            /// <summary>
            /// Set the coordinate system
            /// </summary>
            /// <param name="coordinateSystem">The CoordinateSystem</param>
            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
                both.OnSetCoordinateSystem(coordinateSystem);
            }
        }

        /// <summary>
        /// This class is responsible for taking the calls to DrawGeometry and delegating them to the unified
        /// handlers
        /// </summary>
        private class DrawGeometryInput : GeometryPipeline
        {
            /// <summary>
            /// the DrawBoth instance that should be delegated to
            /// </summary>
            private readonly DrawBoth both;

            /// <summary>
            /// Initializes a new instance of the <see cref="DrawGeometryInput"/> class.
            /// </summary>
            /// <param name="both">The both.</param>
            public DrawGeometryInput(DrawBoth both)
            {
                this.both = both;
            }

            /// <summary>
            /// Draw a point in the specified coordinate
            /// </summary>
            /// <param name="position">Next position</param>
            public override void LineTo(GeometryPosition position)
            {
                both.OnLineTo(position);
            }

            /// <summary>
            /// Begin drawing a figure
            /// </summary>
            /// <param name="position">Next position</param>
            public override void BeginFigure(GeometryPosition position)
            {
                both.OnBeginFigure(position);
            }

            /// <summary>
            /// Begin drawing a spatial object
            /// </summary>
            /// <param name="type">The spatial type of the object</param>
            public override void BeginGeometry(SpatialType type)
            {
                both.OnBeginGeometry(type);
            }

            /// <summary>
            /// Ends the current figure
            /// </summary>
            public override void EndFigure()
            {
                both.OnEndFigure();
            }

            /// <summary>
            /// Ends the current spatial object
            /// </summary>
            public override void EndGeometry()
            {
                both.OnEndGeometry();
            }

            /// <summary>
            /// Setup the pipeline for reuse
            /// </summary>
            public override void Reset()
            {
                both.OnReset();
            }

            /// <summary>
            /// Set the coordinate system
            /// </summary>
            /// <param name="coordinateSystem">The CoordinateSystem</param>
            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
                both.OnSetCoordinateSystem(coordinateSystem);
            }
        }
    }
}
