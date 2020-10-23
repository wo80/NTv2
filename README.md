# NTv2
National transformation (NTv2) for [Proj.NET](https://github.com/NetTopologySuite/ProjNet4GeoAPI).

## Description

A concatenated transform of two projected coordinate systems
```
[0] Source CS (PROJ) -> Source CS (GEOG)
[1] Source CS (GEOG) -> Target CS (GEOG)
[2] Target CS (GEOG) -> Target CS (PROJ)
```
gets replaced with
```
[0] Source CS (PROJ) -> Source CS (GEOG)
[1] Source CS (GEOG) -> Grid Transformation -> Target CS (GEOG)
[2] Target CS (GEOG) -> Target CS (PROJ)
```

A concatenated transform of two geographic coordinate systems
```
[0] Source CS (GEOG) -> Source CS (GEOC)
[1] Source CS (GEOC) -> Target CS (GEOG)
```
gets replaced with
```
[0] Source CS (GEOG) -> Grid Transformation -> Target CS (GEOG)
```

Sub-grid functionally isn't implemented completely. Only the first level of sub-grids is considered.

The library was tested only with German grid file `BETA2007.gsb` from http://www.crs-geo.eu/BeTA2007 which converts Gauss-Krueger to ETRS89 / UTM.
