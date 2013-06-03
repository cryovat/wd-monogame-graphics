namespace Winterday.MonoGame.Graphics.Widgets
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Graph : IDisposable
    {
        bool _disposed;

        GraphicsDevice _device;
        BasicEffect _effect;

        float[] _points;

        Vector2 _position;
        Vector2 _size;

        VertexPositionColor[] _vertexData;
        int _primitiveCount;

        public float this[int index]
        {
            get
            {
                return _points[index];
            }
            set
            {
                _points[index] = Math.Max(0, Math.Min(1, value));
                invalidate();
            }
        }

        public int PointCount
        {
            get
            {
                return _points.Length;
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                invalidate();
            }
        }

        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                invalidate();
            }
        }

        public Graph(GraphicsDevice device, GraphType type, int pointCount)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (type != GraphType.Bars)
                throw new ArgumentException(
                    "Only bar type graphs are supported",
                    "type");

            if (pointCount < 2 || pointCount > 200)
                throw new ArgumentException(
                    "Number of points must be between 2 and 200",
                    "pointCount");

            _points = new float[pointCount];

            _device = device;
            _effect = new BasicEffect(device);

            _vertexData = new VertexPositionColor[pointCount * 2 + 1];
            _primitiveCount = pointCount * 2 - 1;

            for (int i = 0; i < _vertexData.Length; i++)
            {
                _vertexData[i].Color = Color.White;
            }
        }

        public void Update(float elapsedSeconds)
        {
        }

        public void Draw(Color color)
        {
            var viewport = _device.Viewport;

            _effect.World = Matrix.Identity;
            _effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 1, 1000);

            _device.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

            foreach (var technique in _effect.Techniques)
            {
                foreach (var pass in technique.Passes)
                {
                    pass.Apply();

                    _device.DrawUserPrimitives(
                        PrimitiveType.TriangleStrip,
                        _vertexData, 0, _primitiveCount);
                }
            }
            _effect.End();
        }

        private void invalidate()
        {
            var top = _position.Y;
            var bottom = top + _size.Y;

            var left = _position.X;
            var right = left + _size.X;

            var width = right - left;
            var height = bottom - top;

            _vertexData[0].Position =
                new Vector3(left, bottom, 0);

            int count = _points.Length * 2;

            for (int i = 0; i < _points.Length; i++)
            {
                float ratio = i == 0 ? 0 : i + 1 == _points.Length ? 1 : (float)i / _points.Length;
                var offset = i * 2;

                var x = width * ratio;
                var y = height * _points[i];

                _vertexData[1 + offset].Position =
                    new Vector3(left + x, bottom - y, 0);

                _vertexData[2 + offset].Position =
                    new Vector3(left + x, bottom, 0);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _effect.Dispose();
                _disposed = true;
            }
        }
    }
}
