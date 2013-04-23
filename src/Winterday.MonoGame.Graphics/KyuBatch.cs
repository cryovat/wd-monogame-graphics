//
// KyuBatch.cs
//
// Author:
//       Jarl Erik Schmidt <github@jarlerik.com>
//
// Copyright (c) 2013 Jarl Erik Schmidt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Winterday.MonoGame.Graphics
{
	using System;

	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	/// <summary>
	/// An alternative to <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch"/> that allows quads to be
	/// positioned and rotated along three axises.
	/// </summary>
	public class KyuBatch : IDisposable
	{
		const int DefaultCapacity = 256;
		const int VerticesPerQuad = 6;

		// 0  3--4
		// |\  \ |
		// | \  \|
		// 1--2  5
		const int TriOne_TopLeft = 0;
		const int TriOne_BottomLeft = 1;
		const int TriOne_BottomRight = 2;
		const int TriTwo_TopLeft = 3;
		const int TriTwo_TopRight = 4;
		const int TriTwo_BottomRight = 5;

		readonly GraphicsDevice _device;
		readonly BasicEffect _effect;
		readonly int _capacity;
		readonly VertexPositionColorTexture[] _vertexData;
		int _count = 0;
		bool _started;

		public Vector3 CameraPosition { get; set; }
		public bool IsOrthographic { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Winterday.MonoGame.Graphics.KyuBatch"/> class.
		/// </summary>
		/// <param name="device">The current graphicsdevice.</param>
		/// <param name="capacity">The maximum number of quads per draw call for the instance.</param>
		public KyuBatch (GraphicsDevice device, int capacity = DefaultCapacity)
		{
			if (device == null)
				throw new ArgumentNullException ("device");

			if (capacity < 1)
				throw new ArgumentException ("Batch must have a capacity of at least one item", "capacity");

			_device = device;
			_capacity = capacity;		

			IsOrthographic = true;
			
			_vertexData = new VertexPositionColorTexture[capacity * VerticesPerQuad];

			_effect = new BasicEffect (device);		                          
		}

		/// <summary>
		/// Unload this instance.
		/// </summary>
		public void Unload ()
		{
			_effect.Dispose ();
		}

		/// <summary>
		/// Reset this instance, discarding any batched draw calls
		/// </summary>
		public void Reset ()
		{
			_effect.Texture = null;
			_started = false;
			_count = 0;
		}

		/// <summary>
		/// Sets necessary graphics modes and prepares the batch to draw.
		/// </summary>
		/// <remarks>Must be called before <see cref="Draw"/>.</remarks>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> if <see cref="Begin()"/>
		/// has already been called.
		/// </remarks>
		public void Begin ()
		{
			Begin (null, null);
		}

		/// <summary>
		/// Sets necessary graphics modes and prepares the batch to draw.
		/// </summary>
		/// <param name="matrixProvider">Provider for world, view and projection matrices</param>
		/// <remarks>Must be called before <see cref="Draw()"/>.</remarks>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> if <see cref="Begin()"/>
		/// has already been called.
		/// </remarks>
		public void Begin (IMatrixProvider matrixProvider)
		{
			Begin (null, matrixProvider);
		}

		/// <summary>
		/// Sets necessary graphics modes and prepares the batch to draw.
		/// </summary>
		/// <param name="texture">The texture for drawn quads</param>
		/// <remarks>Must be called before <see cref="Draw"/>.</remarks>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> if <see cref="Begin()"/>
		/// has already been called.
		/// </remarks>
		public void Begin (Texture2D texture)
		{
			Begin (texture, null);
		}

		/// <summary>
		/// Sets necessary graphics modes and prepares the batch to draw.
		/// </summary>
		/// <param name="texture">The texture for drawn quads</param>
		/// <param name="matrixProvider">Provider for world, view and projection matrices</param>
		/// <remarks>Must be called before <see cref="Draw"/>.</remarks>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> if <see cref="Begin()"/>
		/// has already been called.
		/// </remarks>
		public void Begin (Texture2D texture, IMatrixProvider matrixProvider)
		{
			if (_started) {
				throw new InvalidOperationException ("Begin() has already been called");
			}

			_started = true;

			if (matrixProvider != null) {
				_effect.World = matrixProvider.WorldMatrix;
				_effect.View = matrixProvider.ViewMatrix;
				_effect.Projection = matrixProvider.ProjectionMatrix;
			} else {
				var viewport = _device.Viewport;

				_effect.World = Matrix.Identity;
				_effect.View = Matrix.CreateLookAt (new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
				_effect.Projection = Matrix.CreateOrthographicOffCenter (0, viewport.Width, viewport.Height, 0, 1, 1000);
			}

			_effect.Alpha = 0.5f;
			_effect.DiffuseColor = Color.White.ToVector3 ();
			_effect.SpecularColor = _effect.DiffuseColor;
			_effect.AmbientLightColor = _effect.DiffuseColor;		
			
			_effect.Texture = texture;
			_effect.TextureEnabled = texture != null;
			_effect.VertexColorEnabled = true;

			_effect.LightingEnabled = false;
		}

		/// <summary>
		/// Batches a draw call for a quad with the given parameters.
		/// </summary>
		/// <param name="position">Position of the quad in 3D space</param>
		/// <param name="rotation">Rotation of quad allong 3 axies</param>
		/// <param name="size">The size of the quad</param>
		/// <param name="scale">Scaling to apply to the quad</param>
		/// <param name="sourceRect">Source rectangle (ignored if no texture is specified)</param>
		/// <param name="color">The vertex color for the quad</param>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> if <see cref="Begin()"/>
		/// has not been called.
		/// </remarks>
		public void Draw (Vector3 position, Vector3 rotation, Vector2 size, Vector2 scale, Vector4 sourceRect, Color color)
		{
			Draw (ref position, ref rotation, ref size, ref scale, ref sourceRect, ref color);
		}

		/// <summary>
		/// Batches a draw call for a quad with the given parameters.
		/// </summary>
		/// <param name="position">Position of the quad in 3D space</param>
		/// <param name="rotation">Rotation of quad allong 3 axies</param>
		/// <param name="size">The size of the quad</param>
		/// <param name="scale">Scaling to apply to the quad</param>
		/// <param name="sourceRect">Source rectangle (ignored if no texture is specified)</param>
		/// <param name="color">The vertex color for the quad</param>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> if <see cref="Begin()"/>
		/// has not been called.
		/// </remarks>
		public void Draw (
			ref Vector3 position, ref Vector3 rotation, ref Vector2 size, ref Vector2 scale,ref Vector4 sourceRect, ref Color color
			)
		{
			if (!_started) {
				throw new InvalidOperationException ("Call Begin() first");
			}

			if (_count == _capacity)
				Flush ();

			int offset = _count;

			var transform = Matrix.Identity *
				Matrix.CreateFromYawPitchRoll (rotation.X, rotation.Y, rotation.Z) *
				Matrix.CreateScale (scale.X, scale.Y, 1) *
				Matrix.CreateTranslation (position.X, position.Y, position.Z);

			var x2 = size.X / 2;
			var x1 = -x2;

			var y2 = size.Y / 2;
			var y1 = -y2;

			var tx1 = sourceRect.X;
			var tx2 = sourceRect.X + sourceRect.Z;
			var ty1 = sourceRect.Y;
			var ty2 = sourceRect.Y + sourceRect.W;

			setVertex (x1, y1, tx1, ty1, ref transform, ref color, ref _vertexData [offset + TriOne_TopLeft]);
			setVertex (x2, y1, tx2, ty1, ref transform, ref color, ref _vertexData [offset + TriTwo_TopRight]);
			setVertex (x1, y2, tx1, ty2, ref transform, ref color, ref _vertexData [offset + TriOne_BottomLeft]);
			setVertex (x2, y2, tx2, ty2, ref transform, ref color, ref _vertexData [offset + TriOne_BottomRight]);

			_vertexData [offset + TriTwo_TopLeft] = _vertexData [offset + TriOne_TopLeft];
			_vertexData [offset + TriTwo_BottomRight] = _vertexData [offset + TriOne_BottomRight];

			_count += VerticesPerQuad;
		}

		void setVertex (
			float x, float y, float tx, float ty, ref Matrix transform, ref Color color,
			ref VertexPositionColorTexture vertex
		)
		{

			vertex.Color = color;
			vertex.Position = Vector3.Transform (new Vector3 (x, y, 0), transform);
			vertex.TextureCoordinate = new Vector2 (tx, ty);

		}

		/// <summary>
		/// Draws all the batched quads to the graphics device.
		/// </summary>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> if <see cref="Begin()"/>
		/// has not been called.
		/// </remarks>
		public void End ()
		{

			if (!_started) {
				throw new InvalidOperationException ("Call Begin() first");
			}

			Flush ();

			_started = false;
			_effect.Texture = null;
		}

		void Flush ()
		{
			_device.RasterizerState = new RasterizerState () { CullMode = CullMode.None };

			foreach (var technique in _effect.Techniques) {
				foreach (var pass in technique.Passes) {
					pass.Apply ();

					_device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList,
					                                                       _vertexData, 0, _count);
				}
			}
			_effect.End ();

			_count = 0;
		
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Winterday.MonoGame.Graphics.KyuBatch"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Winterday.MonoGame.Graphics.KyuBatch"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Winterday.MonoGame.Graphics.KyuBatch"/> in an unusable state.
		/// After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="Winterday.MonoGame.Graphics.KyuBatch"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Winterday.MonoGame.Graphics.KyuBatch"/> was occupying.</remarks>
		public void Dispose ()
		{
			_effect.Dispose ();
		}
	}
}

