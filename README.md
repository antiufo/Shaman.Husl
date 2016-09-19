# Shaman.Husl

C# implementation of [HUSL](http://www.husl-colors.org/), Human-friendly HSL

```csharp
using Shaman.Types;

HuslColor c = HuslColor.FromRgb(Color.LightGreen);
c.Hue += 0.2;
c.ToRgb();
```