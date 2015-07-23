using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace KonamiCode
{
	public class KonamiGestureRecognizer : UIGestureRecognizer
	{
		UITextField _konamiKeyboardResponder;
		KonamiGestureRecognizerState _konamiState;
		DateTime _lastGestureTimestamp;
		CGPoint _lastGesturePoint;

		public KonamiGestureRecognizer () : this (null)
		{
		}

		public KonamiGestureRecognizer (UITextField field)
		{
			if (field != null) {
				field.AutocapitalizationType = UITextAutocapitalizationType.None;
				field.AutocorrectionType = UITextAutocorrectionType.No;
				field.AddTarget (OnEditingChanged, UIControlEvent.EditingChanged);
				_konamiKeyboardResponder = field;
			}

			CancelsTouchesInView = false;
		}

		public override bool CanPreventGestureRecognizer (UIGestureRecognizer preventedGestureRecognizer)
		{
			return false;
		}

		public override void Reset ()
		{
			_konamiState = KonamiGestureRecognizerState.None;
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			if (touches.Count > 1) {
				State = UIGestureRecognizerState.Failed;
			} else {
				if (State == UIGestureRecognizerState.Changed) {
					if (_konamiState < KonamiGestureRecognizerState.Right2) {
						var delta = (DateTime.Now - _lastGestureTimestamp);
						if (delta.Seconds > 1.0f) {
							_konamiState = KonamiGestureRecognizerState.Began;
						}
					}
				} else if (State == UIGestureRecognizerState.Possible) {
					State = UIGestureRecognizerState.Began;
					_konamiState = KonamiGestureRecognizerState.Began;
				} else {
					State = UIGestureRecognizerState.Failed;
					return;
				}

				var touch = touches.AnyObject as UITouch;
				_lastGesturePoint = touch.LocationInView (View);
				_lastGestureTimestamp = DateTime.Now;
			}
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			if (_konamiState >= KonamiGestureRecognizerState.Right2) {
				return;
			}

			var delta = (DateTime.Now - _lastGestureTimestamp);
			if (delta.Seconds > 1.0f) {
				State = UIGestureRecognizerState.Failed;
				return;
			}

			bool checkX, checkY;
			AxisForDrag (_konamiState + 1, out checkX, out checkY);

			var touch = touches.AnyObject as UITouch;
			var currentPoint = touch.LocationInView (View);

			if (checkX) {
				if (Math.Abs (currentPoint.X - _lastGesturePoint.X) > 50.0f) {
					State = UIGestureRecognizerState.Failed;
				}
			} else if (checkY) {
				if (Math.Abs (currentPoint.Y - _lastGesturePoint.Y) > 50.0f) {
					State = UIGestureRecognizerState.Failed;
				}
			}
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			if (_konamiState >= KonamiGestureRecognizerState.Right2) {
				return;
			}

			bool xLeft, xRight, yUp, yDown;
			DirectionForState (_konamiState + 1, out xLeft, out xRight, out yUp, out yDown);

			var touch = touches.AnyObject as UITouch;
			var currentPoint = touch.LocationInView (View);

			if (xLeft || xRight) {
				var delta = currentPoint.X - _lastGesturePoint.X;
				if (!(xLeft && delta < -50.0f) && !(xRight && delta > 50)) {
					State = UIGestureRecognizerState.Failed;
					return;
				}
			} else if (yUp || yDown) {
				var delta = currentPoint.Y - _lastGesturePoint.Y;
				if (!(yUp && delta < -50.0f) && !(yDown && delta > 50)) {
					State = UIGestureRecognizerState.Failed;
					return;
				}
			}

			_lastGestureTimestamp = DateTime.Now;
			_konamiState = _konamiState + 1;

			State = UIGestureRecognizerState.Changed;

			if (_konamiState == KonamiGestureRecognizerState.Right2) {
				if (_konamiKeyboardResponder != null) {
					_konamiKeyboardResponder.Text = null;
					_konamiKeyboardResponder.BecomeFirstResponder ();
				} else {
					State = UIGestureRecognizerState.Recognized;
				}
			}
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			State = UIGestureRecognizerState.Failed;
		}

		void OnEditingChanged (object o, EventArgs args)
		{
			var text = _konamiKeyboardResponder.Text ?? String.Empty;

			if (_konamiState == KonamiGestureRecognizerState.Right2 && text.ToLower () == "b") {
				_konamiState = _konamiState + 1;
				State = UIGestureRecognizerState.Changed;
			} else if (_konamiState == KonamiGestureRecognizerState.B && text.ToLower () == "ba") {
				_konamiKeyboardResponder.ResignFirstResponder ();
				_konamiState = _konamiState + 1;
				State = UIGestureRecognizerState.Recognized;
			} else {
				_konamiKeyboardResponder.ResignFirstResponder ();
				State = UIGestureRecognizerState.Failed;
			}
		}

		static void DirectionForState (KonamiGestureRecognizerState state, out bool xLeft, out bool xRight, out bool yUp, out bool yDown)
		{
			xLeft = xRight = yUp = yDown = false;

			switch (state)
			{
				case KonamiGestureRecognizerState.Began:
				case KonamiGestureRecognizerState.Up1:
				case KonamiGestureRecognizerState.Up2:
					yUp = true;
					break;

				case KonamiGestureRecognizerState.Down1:
				case KonamiGestureRecognizerState.Down2:
					yDown = true;
					break;

				case KonamiGestureRecognizerState.Left1:
				case KonamiGestureRecognizerState.Left2:
					xLeft = true;
					break;

				case KonamiGestureRecognizerState.Right1:
				case KonamiGestureRecognizerState.Right2:
					xRight = true;
					break;

				default:
					break;
			}
		}

		static void AxisForDrag (KonamiGestureRecognizerState state, out bool x, out bool y)
		{
			bool xLeft, xRight, yUp, yDown;
			DirectionForState (state, out xLeft, out xRight, out yUp, out yDown);

			x = yUp || yDown;
			y = xLeft || xRight;
		}

		enum KonamiGestureRecognizerState {
			None, Began, Up1, Up2, Down1, Down2, Left1, Right1, Left2, Right2, B, A,
		}
	}
}

