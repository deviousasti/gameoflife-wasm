module GameOfLifeApp

open GameOfLife
open System
open Elmish
open Bolero
open Bolero.Html

let [<Literal>] Dead = 0
let [<Literal>] Living = 1

/// Invert a cell's state
let flip = function Dead -> Living | _Any -> Dead

/// 3x3 block of neighbors 
let neighbors i j (state: _[,]) =
    state.[i-1..i+1, j-1..j+1]

/// for a given i,j return neighbors which are alive, itself
let alive i j state =
    let rest = state |> neighbors i j |> Array2D.sum
    let self = state.[i, j]
    rest - self, self

/// Standard Game of Life rules
let rules = function    
// Any live cell with less than two live neighbours dies.
| 0, Living 
| 1, Living -> Dead

// Any live cell with two or three live neighbours remains living.
| 2, Living 
| 3, Living -> Living

// Any live cell with more than three live neighbours dies.
| _, Living -> Dead

// Any dead cell with exactly three live neighbours becomes a live cell.
| 3, Dead -> Living

// All else remain unchanged
| _, other -> other

type Model = { rows: int; cols: int; state: int[,]; running: bool }

type Message = 
    | Step 
    | Tick
    | Reset
    | Start 
    | Stop
    | Toggle of i: int * j: int

/// Timer for next tick
let timer _ = Async.Sleep 100

let update message (model: Model) =
    let state = model.state
    let model = 
        match message with
        | Reset ->
            { model with state = state |> Array2D.map(fun _ -> Dead) }
        | Tick | Step -> 
            { model with state = state |> Array2D.mapi(fun i j _ -> alive i j state |> rules ) }
        | Toggle (i, j)-> 
            let toggle i' j' self = 
                self |> if i = i' && j = j' then flip else id
            { model with state = state |> Array2D.mapi toggle }
        | Start -> { model with running = true }
        | Stop  -> { model with running = false }
    
    let command = 
        match message with
        | Start | Tick when model.running ->
            // Start scheduling recursively
            Cmd.ofAsync timer () (fun _ -> Tick) (fun _ -> Stop)
        | _ -> Cmd.none

    model, command
   

let view (model: Model) dispatch =    
    let rows, cols, state, running = model.rows, model.cols, model.state, model.running 
    
    main [] [
        h1 [] [text "Conway's Game of Life"]       
        table [] [
            for i in 2..rows do
                tr [] [
                    for j in 2..cols do
                        let cell = state.[i, j]
                        let css = if cell = Dead then "dead" else "live"
                        td [
                            attr.``class`` css
                            on.click (fun _ -> dispatch (Toggle (i, j)))                            
                        ] [text " "]
                ]
        ]            
        nav [] [
            button [on.click (fun _ -> dispatch Step)] [text "Step"]
            button [on.click (fun _ -> dispatch Reset)] [text "Clear"]
            cond running <| function
            | true  -> button [on.click (fun _ -> dispatch Stop)] [text "Pause"]
            | false -> button [on.click (fun _ -> dispatch Start)] [text "Play"]
        ]
    ]    

type MyApp() =
    inherit ProgramComponent<Model, Message>()
    let size = 15
    let rnd = new Random()
    let initModel m n = 
        { 
            rows = m; cols = n; 
            running = false
            // account for neighbors outside viewport, two above, two below
            state = Array2D.init (m + 4) (n + 4) (fun _ _ -> rnd.Next(2)) 
        }
    
    override this.Program =    
        let model = initModel size size
        // Start our applicatin with our model, our updater and view
        Program.mkProgram (fun _ -> model, Cmd.none) update view        

