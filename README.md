# Tanker Framework

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E120FY7)

Feel free to use in your own projects. If you do, please consider crediting me and including my Ko-Fi link: https://ko-fi.com/sokyran

There's built-in native Multiplayer support.

To set the textures for gizmos, look into included Textures directory - it'll contain 2 text files. You need to use graphical files supported by RimWorld with the same name - but obviously different extension thatn txt.

## Adding the comp

To use it, you must add the component to your thing:

```xml
<li Class="CompTanker.CompProperties_Tanker">
  <!-- Supports `Fuel`, `Oil`, `Water, and `Helixien` values-->
  <!-- Might add more in the future -->
  <contents>Fuel</contents>
  <!-- Default values -->
  <storageCap>10000</storageCap>
  <fillAmount>0.5</fillAmount>
  <drainAmount>0.5</drainAmount>
</li>
```

Or alternatively, if you want to store multiple different things:

```xml
<li Class="CompTanker.CompProperties_Tanker">
  <!-- Supports `Fuel`, `Oil`, `Water, and `Helixien` values-->
  <!-- If there's only a single entry with a value of `All`, it'll automatically support all storage types -->
  <!-- Might add more in the future -->
  <contents>
	<li>Fuel</li>
	<li>Water</li>
  </contents>
  <!-- Default values -->
  <storageCap>10000</storageCap>
  <fillAmount>0.5</fillAmount>
  <drainAmount>0.5</drainAmount>
  <fillGizmoPath></fillGizmoPath>
</li>
```

## Fullness display

If you want your thing to display a bar displaying current fill level when selected, you must set ThingDef's drawerType to MapMeshAndRealTime like so:
```xml
<drawerType>MapMeshAndRealTime</drawerType>
```
Please note, at the moment it only works with Rimefeller/Bad Hygiene.

## Mod specific components

To use this component in your things, make sure your thing also has appropriate component from the mod you use.

### Rimefeller:
```xml
<!-- SRTS ships already have it -->
<li Class="Rimefeller.CompProperties_Pipe"/>
```

### Bad Hygiene:
```xml
<li Class="DubsBadHygiene.CompProperties_Pipe">
  <!-- Yes, sewage mode is used for water too -->
  <mode>Sewage</mode>
</li>
```

### Vanilla Furniture Expanded - Power:
```xml
<li Class="GasNetwork.CompProperties_Gas">
  <compClass>GasNetwork.CompGas</compClass>
</li>
```
