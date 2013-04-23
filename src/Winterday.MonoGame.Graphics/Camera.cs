//
// Camera.cs
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
	/// Defines a camera
	/// </summary>
	public class Camera : IMatrixProvider
	{
		Vector3 _position;
		Vector3 _target;

		Matrix _world;
		Matrix _view;
		Matrix _projection;

		bool _isOrthographic;

		readonly GraphicsDevice _device;
		readonly BoundingFrustum _frustrum = new BoundingFrustum(Matrix.Identity);

		/// <summary>
		/// Gets or sets the position of the camera.
		/// </summary>
		/// <value>The position.</value>
		public Vector3 Position {
			get {
				return _position;
			}
			set {
				if (_position != value) 
				{
					_position = value;
					recalculateMatrices ();
				}
			}
		}

		/// <summary>
		/// Gets or sets the target of the camera.
		/// </summary>
		/// <value>The target.</value>
		public Vector3 Target {
			get {
				return _target;
			}
			set {
				if (_target != value)
				{
					_target = value;
					recalculateMatrices ();
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the camera uses orthographic projection.
		/// </summary>
		/// <value><c>true</c> if the camera is orthographic; otherwise, <c>false</c>.</value>
		public bool IsOrthographic {
			get {
				return _isOrthographic;
			}
			set {
				if (!value) {
					throw new InvalidOperationException("Non-orthographic camera mode is currently not supported");
				}
				else if (_isOrthographic != value)
				{
					_isOrthographic = value;
					recalculateMatrices ();
				}
			}
		}

		/// <summary>
		/// Gets the current calculated world matrix
		/// </summary>
		/// <value>The world matrix.</value>
		public Matrix WorldMatrix
		{
			get {
				return _world;
			}
		}

		/// <summary>
		/// Gets the current view matrix based on position and target
		/// </summary>
		/// <value>The view matrix.</value>
		public Matrix ViewMatrix
		{
			get {
				return _view;
			}
		}

		/// <summary>
		/// Gets the current projection matrix, based on viewport size and projection mode
		/// </summary>
		/// <value>The projection matrix.</value>
		public Matrix ProjectionMatrix
		{
			get {
				return _projection;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Winterday.MonoGame.Graphics.Camera"/> class.
		/// </summary>
		/// <param name="device">The current graphics device</param>
		public Camera (GraphicsDevice device)
		{
			if (device == null)
				throw new ArgumentNullException ("device");

			_device = device;
			
			Reset ();
		}

		/// <summary>
		/// Checks if a given point is within the current bounding frustrum
		/// </summary>
		/// <returns><c>true</c> if the frustrum contaisn the given point; otherwise, <c>false</c>.</returns>
		/// <param name="point">An arbitrary position within 3D space.</param>
		public bool IsVisible(Vector3 point)
		{
			return _frustrum.Contains (point) == ContainmentType.Contains;
		}


		/// <summary>
		/// Reset the position and target of the camera.
		/// </summary>
		public void Reset()
		{
			_position = new Vector3 (0, 0, 1);
			_target = Vector3.Zero;

			recalculateMatrices ();
		}

		void recalculateMatrices() {
			var viewport = _device.Viewport;
			_world = Matrix.Identity;
			_view = Matrix.CreateLookAt (_position, Vector3.Zero, Vector3.Up);
			_projection = Matrix.CreateOrthographicOffCenter (0, viewport.Width, viewport.Height, 0, 1, 1000);
			_frustrum.Matrix = _view * _projection;			
		}
	}
}

