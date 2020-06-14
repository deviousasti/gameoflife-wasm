# Conway's Game of Life 

This is an implementation of Conway's Game of Life in F# using [Bolero](https://github.com/fsbolero/Bolero), which runs a compiled F# assembly entirely in Web Assembly. This is different from [F# Fable](https://fable.io) which transpiles to Javascript.

There are no limitations on what portions of the library are available, rather any library can be used in Bolero, as evidenced by the use of `Array2D` functions.


![recording](https://user-images.githubusercontent.com/2375486/84602711-f5cedb00-aea6-11ea-8b60-720467cfdf9d.gif)

## Implementation

There is no HTML component; the entire Model-View-Update (MVU) application is written in Elmish in `Main.fs`.
The model is immutable, so all updates are transformations. (most implementations which use double-buffering and array mutations). 

## Observations

Interop with the JS runtime seems to be expensive, while calculating the diff is not costly.

The internal state is a 2D array. Since model state in Elmish is never mutated, it must be recreated every time.
```
 { model with state = state |> Array2D.mapi(fun i j _ -> alive i j state |> rules) }
``` 
So enough memory pressure is generated to cause a GC.

![image](https://user-images.githubusercontent.com/2375486/84602915-324f0680-aea8-11ea-99d0-67e59c105e63.png)

This is a minor GC and very fast (less than 1ms).

