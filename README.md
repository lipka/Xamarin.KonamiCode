# Xamarin.KonamiCode

A Xamarin.iOS (MonoTouch) implementation of a Konami code gesture recognizer. Supports the new Unified API.

Adapted from the original [DRKonamiCode](https://github.com/objectiveSee/DRKonamiCode) project.

## Usage

``` c#
using KonamiCode;

...

var konamiField = new UITextField ();
View.AddSubview (konamiField);
View.AddGestureRecognizer (new KonamiGestureRecognizer (konamiField));

```

## Installation

Just add KonamiGestureRecognizer.cs to your project. Boom. Done.

## Requirements

Xamarin.KonamiCode is tested on iOS7 and above.

## Contact

Lukas Lipka

- http://github.com/lipka
- http://twitter.com/lipec
- http://lukaslipka.com

## License

Xamarin.KonamiCode is available under the [MIT license](LICENSE). See the LICENSE file for more info.
