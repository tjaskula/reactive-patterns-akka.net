module Messages

    type PlayMovieMessage = {MovieTitle : string; UserId : int}

    type StopMovieMessage () = class end