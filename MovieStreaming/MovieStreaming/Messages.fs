module Messages

    type PlayMovieMessage = {MovieTitle : string; UserId : int}

    type StopMovieMessage () = class end

    // more functional definition
    type Message =
        | PlayMovie of PlayMovieMessage
        | StopMovie

    type ActorState =
        | Playing of string
        | Stopped of string