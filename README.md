# UMG Recklass Rekkids search program

## Praticalities
### To build

```sh
$ cd RecklassRekkids
$ dotnet clean && dotnet build
$ ./bin/Debug/net7.0/RecklassRekkids 'data/contracts.txt' 'data/partners.txt' 'YouTube' '06-01-2012'
```

### To test

```sh
$ cd RecklassRekkids.Test
$ dotnet test
```

## Optional results and the resulting optionals

the `OptionalBuilder` and corresponding optional computation blocks (the code in curly braces proceeded by ‘optional’ warrants some explanation.

first, the motivation...

### Exceptions are crass

what is the problem we’re trying to solve here?  handling actions that might fail, like parsing a date from a user-provided string.  that is, exceptions, which are normally handled through the normal exception handling mechanisms common to languages like C#, Java, and such.

So, not mutating values is a big FP thing.  and C#-style exceptions do always entail some amount of mutation.

but more than the whole immutability thing, there’s other arguably undesirable - or at least un-FPy - stuff afoot with C#-style exception handling.  (perhaps we should say imperative-style so as not to be unfair to C#.)

Imperative-style exception propagation also breaks normal control flow - runs kinda orthogonal to it, really.  even with exception-typing in method signatures, it’s still nontrivial to trace through the whole control flow space.  even with all it’s debatable crassness, normal control flow in imperative code can usually be read off lexically, more or less.  Exception propagation is intrinsically dynamic and can almost never be read off so easily.

### Declarative declared

And so we arrive at another 25-cent FP word - declarative.  and this teases out at least as much of the distinction between FP and imperative coding as immutability.

So, in imperative code (as just stated), you can read off the (non-exceptional) control flow from the code, more or less, just reading line-to-line.  however imperfectly.

but in declarative code, you more read off the intention of the code.

And it’s not as esoteric as it might sound.

SQL is a good example of a declarative languages (specifically the querying part of SQL, i.e. `select` statements).  Very roughly, a SQL query would be declarative in the sense that you "declare" what columns you want to project, joins, `where` clause, etc, you want without telling the database how to actually go and get the data.  the imperative approach to the same task would roughly be to manually implement whatever is laid out in the explain plan for that query.

So, back to exceptions.

### A slightly rambly spiel on Monads

>Or at least, how to sort-of pull monads apart from the other more easily named parts

Exception handling has a way of dominating your code like a rude house guest.  `try`-`catch`-`finally` blocks are loud, force additional scope-nesting, tend to speparate declaration from assignment, and the exception names are always really, really long (it seems like).  what's more, it  always feels, to greater or lesser degree, like the exception handling is tacked on top of the "real" logic of a program.

And really, it makes sense that it would feel that way - you've got the main logic, which does whatever it does.  But the "real" or "main" program, which largely chugs along under its own self-consistent flow, just happens to make a few calls that may fail for reasons having nothing to do with the program - either its logic and internal state.  the program could run twice, with identical state just before such a call, and one call fails and the other doesn't.

So that's probably a sufficiently belabored point.

but here we have a third $0.25-FP buzzword - "purity".  Or, to be more precise - ["referential transparancy"](https://levelup.gitconnected.com/pure-function-vs-referential-transparency-7192553d9d1).

So, back to OptionalBuilder.

An `Optional<T>` (synonomous with `Maybe`, a term found in other settings - `Haskell` being a big one) in `F#` can be in one of two possible states.  it is either `None` or `Some(x:T)`.  In alot of ways, this is what `F#` does (or prefers, at least) instead of nulls.  If `T` is a reference type and `None` maps to `null`, then `Some(x)` maps to a non-null reference to the value `x`.

But `Optional` can also be used to represent the result of a call that might fail, and often is.  If a call succeeds and returns value `x`, then that becomes `Some(x)`; otherwise, `None`.

(Note that this latter use of `Optional` is probably not appropriate, at least not ideal.  `Result`, with a corresponding builder and computation expression, is really more suited to exception propagation.)

the `optional` computation expression (everything within the curly braces that open right after `optional` in the code) is an attempt to tease out our "pure" code - that which succeeds or fails only in the world of its own solopsism, from the fickle mercies of the so-called "outside world" of network requests and sketchy filesystems and good old user input.

when we extract the pure part of a program successfully, what remains is often called a monad.  Since _monad_ is one of the most ardently abused terms in programming/math, i'll defer to [this for a description](https://fsprojects.github.io/FSharpPlus/abstraction-monad.html).  Just know that this is one description out of very many of "monad".  

>one of the best one-word approximate descriptions of "monad" is a "context".

But yeah, the optional is meant to be the monad that would allow our possibly-`None` values to pass through a `|>` pipeline without any taint of optional syntax in the monad.  and for `F#`, you can read "monad" as "computation expression".  It can all be very confusing, at least from the monad side.  but `F#` computation expressions really are a clean realization of the _monad_ concept (to me, anyway).

### Railway Oriented Programming

Finally, I think the (original) article that I stumbled across, which introduced me to this way of thinking was [Railway Oriented Programming](https://fsharpforfunandprofit.com/posts/recipe-part2/). I highly recommend this to anyone interested in FP or F#.















