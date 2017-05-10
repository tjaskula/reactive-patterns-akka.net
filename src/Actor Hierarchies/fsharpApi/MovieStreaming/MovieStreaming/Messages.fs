module Messages

    type PlayMovieMessage = {MovieTitle : string; UserId : int}

    type StopMovieMessage = {UserId : int}

    // more functional definition
    type Message =
        | PlayMovie of PlayMovieMessage
        | StopMovie of StopMovieMessage

    type ActorState =
        | Playing of string
        | Stopped of string