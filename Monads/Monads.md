# Monads
## What is Monad
- A single unit
- A design pattern that allows structing programs generically while automating away boilerplate code needed by the program logic
- "裹在被子里打牌"
- wtf ???

# A world without Monad
We want to program only using functions.  

A pure function is a function where the return value is only determined by its input, without observable side effects. It means, a specific input will lead to a specific output.  

However, let's take random generator as an example:   

A random generator will access the random seed, which has "side effects": there're lots of states which are irrelavent with our code in these operations, and they are not explicit declared.  
  
`nextRandom` accepts no input but it will output different numbers, and it's relavent with time.   

```haskell
nextRandom :: Int -- Shouldn't it be a constant value?
```

In order to program it in a "functional" way, we need to write a function which explicit declares all the states in parameters. And we need to pass those states via parameters, and in the end we can get a specific output from a specific input.  

It's so complicated and hard to program.

# What is Monad again and Why Monad
We want to use functions which have "side effects".  

We don't want to handle all the states and environment by ourselves.  

Therefore, we can wrap an operation which has side effects and its states and values in a "package" and regard it as a new operation `Random Int`, where random is the package.  

```haskell
nextRandom :: Random Int
```

However, `Random Int` is not a `Int`, and `Random Int` + `Int` is illegal. We only care about pertinent values in the `Random` but not the `Random` itself, so how can we operate values in the `Random` without unwrap it? (If we unwrap it from `Random`, there will be states in our code again)  

Solution: compose them to construct a chain!  

We cannot do `Random Int` + `Int`, but we can do it in the `Random` by operate the pertinent values immediately after `nextRandom`.  

We have 
```haskell
nextRandom :: Random Int
plusOne :: Int -> Int
```

We use a `bind` (`>>=`) and a `return`:
```haskell
(>>=) :: Random Int -> (Int -> Random Int) -> Random Int
return :: Int -> Random Int

nextRandom() >>= (return plusOne)
-- Actually it's unnecessary. We can use plusOne <$> nextRandom instead (functor). Monad can be used for something like "randomStringWithLength" to generate a random string with specific length.
```

Finally we get a `Random Int`!

# Who are Monads
- `Maybe` is Monad
- `IO` is Monad
- `List` is Monad
- `Random` is Monad
- `Either` is Monad
- `Try` is Monad
- `Reader` is Monad
- `Writer` is Monad
- `State` is Monad
- ......

# Maybe
Take 'Maybe' as an example.
```haskell
data Maybe a = Just a | Nothing
```

# Functor
'Maybe' is functor!
```haskell
-- fmap
(<$>) :: (a -> b) -> Maybe a -> Maybe b
f <$> (Just x) = Just (f x)
_ <$> Nothing = Nothing
```
So we have:
```haskell
f :: a -> b
(<$>) f :: Maybe a -> Maybe b
```

# Applicative
'Maybe' is applicative!
```haskell
-- inherit relation
Functor => Applicative

pure :: a -> Maybe a
pure = Just

-- apply
(<*>) :: Maybe (a -> b) -> Maybe a -> Maybe b
Nothing <*> _  = Nothing
(Just f) <*> x = fmap f x
```
So we have:
```haskell
x :: Num
f :: Maybe (a -> b)
pure x :: Maybe Num
apply f (pure x) :: Maybe Num
```

# Monad
'Maybe' is Monad!
```haskell
-- inherit relation
Applicative => Functor

-- No side effects are special cases with side effects
return :: a -> Maybe a
return x = Just x

-- bind
(>>=) :: Maybe a -> (a -> Maybe b) -> Maybe b
Nothing >>= _ = Nothing
(Just x) >>= f = f x

-- then
(>>) :: Maybe a -> Maybe b -> Maybe b
Nothing >> _ = Nothing
_ >> Nothing = Nothing
_ >> m = m

fail :: String -> Maybe a
fail _ = Nothing
```

# IO
```haskell
printW :: [String] -> IO ()
printW [] = return ()
printW (h:t) = putStrLn h >> printW t   
    
-- getLine :: IO String
-- words :: String -> [String]
f :: IO [String]
f = words <$> getLine

main :: IO ()
main = f >>= printW
```
With syntax sugar:
```haskell
printW :: [String] -> IO ()
printW [] = return ()
printW (h:t) = do
    putStrLn h
    printW t
    
f :: IO ()
f = do
    l <- getLine
    let w = words l
    printW w
```

# Appendix: Monads in C&#35;
C&#35; is a general-purpose, multi-paradigm programming language encompassing strong typing, lexically scoped, imperative, declarative, functional, generic, object-oriented, and component-oriented programming disciplines.  

Now let's see Monads in C&#35;!
# `Task<T>` is Monad
Asynchronous invoke
```csharp
Task<Result> QueryDatabase()
{
    return Task.Run(() => 
    {
        ConnectDatabse().ContinueWith(conn =>
        {
            OpenDatabase(conn).ContinueWith(db =>
            {
                db.Query("select * from Table1").ContinueWith(result =>
                {
                    return new Result
                    {
                        ......
                    };
                });
            });
        });
    });
}
```

```csharp
async Task<Result> QueryDatabase()
{
    var conn = await ConnectDatabaseAsync();
    var db = await conn.OpenAsync();
    var result = await db.QueryAsync("select * from Table1");
    return new Result
    {
        ......
    };
}
```

# `IEnumerable<T>` + `SelectMany` is Monad
`IEnumerable<T>`:
```csharp
IEnumerable<int> Fibonacci()
{
    var current = 1;
    var next = 1;
    while (true)
    {
        yield return current;

        var tmp = next;
        next += current;
        current = next;
    }
}

foreach (var i in Fibonacci())
{
    Console.Write($"{i} ");
}
// 1 1 2 3 5 8 13 ....
```
In Haskell:
```haskell
-- bind
(>>=) :: [a] -> (a -> [b]) -> [b]
```
In C&#35;:
```csharp
// bind
public static IEnumerable<B> SelectMany<A, B>(this IEnumerable<A> first, Func<A, IEnumerable<B>> selector);
```

# `Nullable<T>` is Monad
```csharp
// int and Nullable<int>
int t1 = 1; // t1 is not nullable
int? t2 = 1;
t2 = null; // t2 is nullable
```

# Implement Monads
Let's implement some Monads.
- `Try` √
- `Either` √
- `IO`
- `Reader`
- `Writer`
- `State`
- `Reader + Writer + State: RWS`

......

# Unit
```csharp
// A type with only one value, itself (instead of Void).
public struct Unit
{
    public static readonly Unit Default = new Unit();

    // Performs an action which instead of returning void will return Unit
    public static Unit Return(Action action)
    {
        if (action == null) throw new ArgumentNullException("action");

        action();
        return Default;
    }
}
```

# Try
```csharp
// The Try monad delegate
public delegate TryResult<T> Try<T>();

// Holds the state of the error monad during the bind function. If IsFaulted == true then the bind function will be cancelled.
public struct TryResult<T>
{
    public readonly T Value;
    public readonly Exception Exception;

    public TryResult(T value)
    {
        Value = value;
        Exception = null;
    }

    public TryResult(Exception e)
    {
        if (e == null) throw new ArgumentNullException("e");
        Exception = e;
        Value = default(T);
    }

    public static implicit operator TryResult<T>(T value)
    {
        return new TryResult<T>(value);
    }

    // True if faulted
    public bool IsFaulted => Exception != null;
}

// Extension methods for the error monad
public static class TryExt
{
    public static TryResult<T> Try<T>(this Try<T> self)
    {
        try
        {
            return self();
        }
        catch (Exception e)
        {
            return new TryResult<T>(e);
        }
    }

    // Return a valid value regardless of the faulted state
    public static T GetValueOrDefault<T>(this Try<T> self)
    {
        var res = self.Try();
        if (res.IsFaulted)
            return default(T);
        else
            return res.Value;
    }

    // Return the value of the monad and it throws an exception if the monad is in a faulted state.
    public static T Value<T>(this Try<T> self)
    {
        var res = self.Try();
        if (res.IsFaulted)
            throw new InvalidOperationException("The try monad has no value. It holds an exception of type: " + res.GetType().Name + ".");
        else
            return res.Value;
    }

    // Select
    public static Try<U> Select<T, U>(this Try<T> self, Func<T, U> selector)
    {
        if (selector == null) throw new ArgumentNullException("selector");

        return new Try<U>(() =>
            {
                TryResult<T> resT;
                try
                {
                    resT = self();
                    if (resT.IsFaulted)
                        return new TryResult<U>(resT.Exception);
                }
                catch (Exception e)
                {
                    return new TryResult<U>(e);
                }

                U resU;
                try
                {
                    resU = selector(resT.Value);
                }
                catch (Exception e)
                {
                    return new TryResult<U>(e);
                }

                return new TryResult<U>(resU);
            });
    }

    // SelectMany
    public static Try<V> SelectMany<T, U, V>(
        this Try<T> self,
        Func<T, Try<U>> selector,
        Func<T, U, V> bind
        )
    {
        if (selector == null) throw new ArgumentNullException("selector");
        if (bind == null) throw new ArgumentNullException("bind");

        return new Try<V>(() =>
            {
                TryResult<T> resT;
                try
                {
                    resT = self();
                    if (resT.IsFaulted)
                        return new TryResult<V>(resT.Exception);
                }
                catch (Exception e)
                {
                    return new TryResult<V>(e);
                }

                TryResult<U> resU;
                try
                {
                    resU = selector(resT.Value)();
                    if (resU.IsFaulted)
                        return new TryResult<V>(resU.Exception);
                }
                catch (Exception e)
                {
                    return new TryResult<V>(e);
                }

                V resV;
                try
                {
                    resV = bind(resT.Value, resU.Value);
                }
                catch (Exception e)
                {
                    return new TryResult<V>(e);
                }

                return new TryResult<V>(resV);
            }
        );
    }
}

public class Try
{
    // Empty
    public static Try<T> Empty<T>()
    {
        return () => default(T);
    }
}
```

# Either
```csharp
public class Either
{
    // Construct an Either Left monad
    public static Either<L, R> Left<L, R>(L left)
    {
        return new Either<L, R>(left);
    }

    // Construct an Either Right monad
    public static Either<L, R> Right<L, R>(R right)
    {
        return new Either<L, R>(right);
    }

    // Monadic zero
    public static Either<L, R> Mempty<L, R>()
    {
        return new Either<L, R>(default(R));
    }
}

/*
The Either monad represents values with two possibilities: a value of Left or Right Either is sometimes used to represent a value which is either correct or an error, by convention, 'Left' is used to hold an error value 'Right' is used to hold a correct value.
So you can see that Either has a very close relationship to the Error monad.  However, the Either monad won't capture exceptions.  Either would primarily be used for known error values rather than exceptional ones.
Once the Either monad is in the Left state it cancels the monad bind function and returns immediately.
*/
public struct Either<L, R> : IEquatable<Either<L, R>>
{
    static readonly bool IsAppendable = typeof(IAppendable<R>).GetTypeInfo().IsAssignableFrom(typeof(R).GetTypeInfo());

    readonly L left;
    readonly R right;

    // Returns true if the monad object is in the Left state
    public readonly bool IsLeft;

    // Left constructor
    internal Either(L left)
    {
        IsLeft = true;
        this.left = left;
        this.right = default(R);
    }

    // Right constructor
    internal Either(R right)
    {
        IsLeft = false;
        this.right = right;
        this.left = default(L);
    }

    // Returns true if the monad object is in the Right state
    public bool IsRight => !IsLeft;
    
    // Get the Left value and it throws an exception if the object is in the Right state
    public L Left
    {
        get
        {
            if (!IsLeft)
                throw new InvalidOperationException("Not in the left state");
            return left;
        }
    }

    // Get the Right value and it throws an exception if the object is in the Left state
    public R Right
    {
        get
        {
            if (!IsRight)
                throw new InvalidOperationException("Not in the right state");
            return right;
        }
    }

    /* 
    Pattern matching method for a branching expression
    Right: Action to perform if the monad is in the Right state
    Left: Action to perform if the monad is in the Left state 
    */
    public T Match<T>(Func<R, T> Right, Func<L, T> Left)
    {
        if (Right == null) throw new ArgumentNullException("Right");
        if (Left == null) throw new ArgumentNullException("Left");
        return IsLeft
            ? Left(this.Left)
            : Right(this.Right);
    }

    /*
    Pattern matching method for a branching expression and it throws an exception if the object is in the Left state
    right: Action to perform if the monad is in the Right state
    */
    public T MatchRight<T>(Func<R, T> right)
    {
        if (right == null) throw new ArgumentNullException("right");
        return right(this.Right);
    }

    /*
    Pattern matching method for a branching expression and it throws an exception if the object is in the Right state
    left: Action to perform if the monad is in the Left state
    */
    public T MatchLeft<T>(Func<L, T> left)
    {
        if (left == null) throw new ArgumentNullException("left");
        return left(this.Left);
    }

    /*
    Pattern matching method for a branching expression
    Returns the defaultValue if the monad is in the Left state
    right: Action to perform if the monad is in the Right state
    */
    public T MatchRight<T>(Func<R, T> right, T defaultValue)
    {
        if (right == null) throw new ArgumentNullException("right");
        if (IsLeft)
            return defaultValue;
        return right(this.Right);
    }

    /*
    Pattern matching method for a branching expression
    Returns the defaultValue if the monad is in the Right state
    left: Action to perform if the monad is in the Left state
    */
    public T MatchLeft<T>(Func<L, T> left, T defaultValue)
    {
        if (left == null) throw new ArgumentNullException("left");
        if (IsRight)
            return defaultValue;
        return left(this.Left);
    }


    /*
    Pattern matching method for a branching expression
    Right: Action to perform if the monad is in the Right state
    Left: Action to perform if the monad is in the Left state
    */
    public Unit Match(Action<R> Right, Action<L> Left)
    {
        if (Right == null) throw new ArgumentNullException("Right");
        if (Left == null) throw new ArgumentNullException("Left");

        var self = this;

        return Unit.Return(() =>
        {
            if (self.IsLeft)
                Left(self.Left);
            else
                Right(self.Right);
        });
    }

    /*
    Pattern matching method for a branching expression and it throws an exception if the object is in the Left state
    right: Action to perform if the monad is in the Right state
    */
    public Unit MatchRight(Action<R> right)
    {
        if (right == null) throw new ArgumentNullException("right");
        var self = this;
        return Unit.Return(() => right(self.Right));
    }

    /*
    Pattern matching method for a branching expression and it throws an exception if the object is in the Right state
    left: Action to perform if the monad is in the Left state
    */
    public Unit MatchLeft(Action<L> left)
    {
        if (left == null) throw new ArgumentNullException("left");
        var self = this;
        return Unit.Return(() => left(self.Left));
    }

    /*
    Monadic append
    If the left-hand side or right-hand side are in a Left state, then Left propogates
    */
    public static Either<L, R> operator +(Either<L, R> lhs, Either<L, R> rhs)
    {
        return lhs.Mappend(rhs);
    }

    /*
    Left coalescing operator
    Returns the left-hand operand if the operand is not Left; otherwise it returns the right hand operand.
    In other words it returns the first valid option in the operand sequence.
    */
    public static Either<L, R> operator |(Either<L, R> lhs, Either<L, R> rhs)
    {
        return lhs.IsRight
            ? lhs
            : rhs;
    }

    /*
    Returns the right-hand side if the left-hand and right-hand side are not Left.
    In order words every operand must hold a value for the result to be Right.
    In the case where all operands return Left, then the last operand will provide its value.
    */
    public static Either<L, R> operator &(Either<L, R> lhs, Either<L, R> rhs)
    {
        return lhs.IsRight && rhs.IsRight
            ? rhs
            : lhs.IsRight
                ? rhs
                : lhs;
    }

    /*
    Monadic append
    If the left-hand side or right-hand side are in a Left state, then Left propagates
    */
    public Either<L, R> Mappend(Either<L, R> rhs)
    {
        if (IsLeft)
        {
            return this;
        }
        else
        {
            if (rhs.IsLeft)
            {
                return rhs.Left;
            }
            else
            {
                if (IsAppendable)
                {
                    var lhs = this.Right as IAppendable<R>;
                    return new Either<L, R>(lhs.Append(rhs.Right));
                }
                else
                {
                    var test = default(R);
                    var result = test switch
                    {
                        long _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToInt64(right) + Convert.ToInt64(rhs.right)), typeof(R))),
                        ulong _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToUInt64(right) + Convert.ToUInt64(rhs.right)), typeof(R))),
                        int _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToInt32(right) + Convert.ToInt32(rhs.right)), typeof(R))),
                        uint _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToUInt32(right) + Convert.ToUInt32(rhs.right)), typeof(R))),
                        short _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToInt16(right) + Convert.ToInt16(rhs.right)), typeof(R))),
                        ushort _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToUInt16(right) + Convert.ToUInt16(rhs.right)), typeof(R))),
                        decimal _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToDecimal(right) + Convert.ToDecimal(rhs.right)), typeof(R))),
                        double _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToDouble(right) + Convert.ToDouble(rhs.right)), typeof(R))),
                        float _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToSingle(right) + Convert.ToSingle(rhs.right)), typeof(R))),
                        char _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToChar(right) + Convert.ToChar(rhs.right)), typeof(R))),
                        byte _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToByte(right) + Convert.ToByte(rhs.right)), typeof(R))),
                        string _ => new Either<L, R>((R)Convert.ChangeType((Convert.ToString(right) + Convert.ToString(rhs.right)), typeof(R))),
                        _ => throw new InvalidOperationException($"Type {typeof(R).Name} is not appendable. Consider implementing the IAppendable interface.")
                    }
                    return result;
                }
            }
        }
    }

    // Either -> IEnumerable<R>
    public IEnumerable<R> AsEnumerable()
    {
        if (IsRight)
            yield return Right;
        else
            yield break;
    }

    // Either -> infinite IEnumerable<R>
    public IEnumerable<R> AsEnumerableInfinite()
    {
        if (IsRight)
            while (true) yield return Right;
        else
            yield break;
    }

    public static bool operator ==(Either<L, R> lhs, Either<L, R> rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Either<L, R> lhs, Either<L, R> rhs)
    {
        return !lhs.Equals(rhs);
    }

    public static bool operator ==(Either<L, R> lhs, L rhs)
    {
        return lhs.Equals(new Either<L, R>(rhs));
    }

    public static bool operator !=(Either<L, R> lhs, L rhs)
    {
        return !lhs.Equals(new Either<L, R>(rhs));
    }

    public static bool operator ==(Either<L, R> lhs, R rhs)
    {
        return lhs.Equals(new Either<L, R>(rhs));
    }
    
    public static bool operator !=(Either<L, R> lhs, R rhs)
    {
        return !lhs.Equals(new Either<L, R>(rhs));
    }

    public static implicit operator Either<L, R>(L left)
    {
        return new Either<L, R>(left);
    }

    public static implicit operator Either<L, R>(R right)
    {
        return new Either<L, R>(right);
    }

    public override int GetHashCode()
    {
        return IsLeft
            ? Left == null ? 0 : Left.GetHashCode()
            : Right == null ? 0 : Right.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        else
        {
            if (obj is Either<L, R>)
            {
                var rhs = (Either<L, R>)obj;
                return IsRight && rhs.IsRight
                    ? Right.Equals(rhs.Right)
                    : IsLeft && rhs.IsLeft
                        ? true
                        : false;
            }
            else if (obj is R)
            {
                var rhs = (R)obj;
                return IsRight
                    ? Right.Equals(rhs)
                    : false;
            }
            else if (obj is L)
            {
                var rhs = (L)obj;
                return IsLeft
                    ? Left.Equals(rhs)
                    : false;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Equals(Either<L, R> rhs)
    {
        return Equals((object)rhs);
    }

    public bool Equals(L rhs)
    {
        return Equals((object)rhs);
    }

    public bool Equals(R rhs)
    {
        return Equals((object)rhs);
    }
}

// Either extension methods
public static class EitherExt
{
    public static Either<L, UR> Select<L, TR, UR>(
        this Either<L, TR> self,
        Func<TR, UR> selector)
    {
        if (selector == null) throw new ArgumentNullException("selector");

        if (self.IsLeft)
            return Either.Left<L, UR>(self.Left);

        return Either.Right<L, UR>(selector(self.Right));
    }

    public static Either<L, VR> SelectMany<L, TR, UR, VR>(
        this Either<L, TR> self,
        Func<TR, Either<L, UR>> selector,
        Func<TR, UR, VR> projector)
    {
        if (selector == null) throw new ArgumentNullException("selector");
        if (projector == null) throw new ArgumentNullException("projector");

        if (self.IsLeft)
            return Either.Left<L, VR>(self.Left);

        var res = selector(self.Right);
        if (res.IsLeft)
            return Either.Left<L, VR>(res.Left);

        return Either.Right<L, VR>(projector(self.Right, res.Right));
    }

    public static Either<L, R> Mconcat<L, R>(this IEnumerable<Either<L, R>> ms)
    {
        var value = ms.Head();

        foreach (var m in ms.Tail())
        {
            if (value.IsLeft)
                return value;

            value = value.Mappend(m);
        }
        return value;
    }
}
```