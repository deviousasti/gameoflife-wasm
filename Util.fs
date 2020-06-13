namespace GameOfLife



module Array2D =
    let inline sum arr = 
        let mutable total = LanguagePrimitives.GenericZero
        arr |> Array2D.iter(fun v -> total <- total + v)
        total