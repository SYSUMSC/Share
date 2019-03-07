# Monads
## What is Monad
- A single unit
- A design pattern that allows structing programs generically while automating away boilerplate code needed by the program logic
- "裹在被子里打牌"

Firstly we need to introduce a type 'Maybe':
```haskell
data Maybe a = Just a | Nothing
```
# Functor
'Maybe' is functor!
```haskell
fmap :: (a -> b) -> Maybe a -> Maybe b
fmap f (Just x) = Just (f x)
fmap _ Nothing = Nothing
```
So we have:
```haskell
f :: a -> b
fmap f :: Maybe a -> Maybe b
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

return :: a -> Maybe a
return x = Just x

-- bind
(>>=) :: Maybe a -> (a -> Maybe b) -> Maybe b
Nothing >>= _ = Nothing
(Just x) >>= f = f x

fail :: String -> Maybe a
fail _ = Nothing
```

# Why we need Monad
- We want to program only using functions
- The order of execution of multiple functions is unclear:
    ```haskell
    f :: Num -> Num
    f x = x + 1
    g :: Num -> Num -> Num
    g x y = x / y
    ```
    Solution: compose them, if we want fist g and then f, just write:
    ```haskell
    main :: Num -> Num -> Num
    main x y = f (g x y)
    ```
- But some functions might fail:
    ```haskell
    g 5 0 = ??? -- 5 is divided by 0
    ```
    Solution: We use 'Maybe'
    ```haskell
    g :: Num a => Maybe a -> Maybe a -> Maybe a
    g (Just 5) (Just 1) = Just 5
    g (Just 5) (Just 0) = Nothing
    ```
- But g returns a Maybe, however in f, a 'Maybe' can not be applied to '+' with an 'Integer'
    ```haskell
    main (Just 5) (Just 1) = ???
    -- (Maybe Integer) + Integer ?
    ```
    Solution: we need 'bind', and also make produce Maybe
    ```haskell
    f :: Num a => a -> Maybe a
    f x = Just (x + 1)
    
    main :: Num a => Maybe a -> Maybe a
    main x = (g x) >>= f
    ```
# Monads in C&#35;
C&#35; is a general-purpose, multi-paradigm programming language encompassing strong typing, lexically scoped, imperative, declarative, functional, generic, object-oriented (class-based), and component-oriented programming disciplines.  
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
- `Either`
- `IO`
- `Reader` √
- `Writer` √
- `State` √
- `Reader + Writer + State: RWS` √

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

/// Holds the state of the error monad during the bind function. If IsFaulted == true then the bind function will be cancelled.
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

    // ToString override
    public override string ToString()
    {
        return IsFaulted
            ? Exception.ToString()
            : Value != null
                ? Value.ToString()
                : "[null]";
    }
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

    // Return the value of the monad and throw an exception if the monad is in a faulted state.
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

    // Fluent chaining of Try monads
    public static Try<U> Then<T, U>(this Try<T> self, Func<T, U> getValue)
    {
        if (getValue == null) throw new ArgumentNullException("getValue");

        var resT = self.Try();

        return resT.IsFaulted
            ? new Try<U>(() => new TryResult<U>(resT.Exception))
            : new Try<U>(() =>
                {
                    try
                    {
                        U resU = getValue(resT.Value);
                        return new TryResult<U>(resU);
                    }
                    catch (Exception e)
                    {
                        return new TryResult<U>(e);
                    }
                });
    }

    // Try<T> -> IEnumerable<T>
    public static IEnumerable<T> AsEnumerable<T>(this Try<T> self)
    {
        var res = self.Try();
        if (res.IsFaulted)
            yield break;
        else
            yield return res.Value;
    }

    // Try<T> -> infinite IEnumerable<T>
    public static IEnumerable<T> AsEnumerableInfinite<T>(this Try<T> self)
    {
        var res = self.Try();
        if (res.IsFaulted)
            yield break;
        else
            while (true) yield return res.Value;
    }

    // Mappend
    public static Try<T> Mappend<T>(this Try<T> lhs, Try<T> rhs)
    {
        if (rhs == null) throw new ArgumentNullException("rhs");

        return () =>
        {
            var lhsValue = lhs();
            if (lhsValue.IsFaulted) return lhsValue;

            var rhsValue = rhs();
            if (rhsValue.IsFaulted) return rhsValue;

            bool IsAppendable = typeof(IAppendable<T>).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());

            if (IsAppendable)
            {
                var lhsAppendValue = lhsValue.Value as IAppendable<T>;
                return lhsAppendValue.Append(rhsValue.Value);
            }
            else
            {
                var test = default(T);

                var result = test switch
                {
                    long _ => (T)Convert.ChangeType((Convert.ToInt64(lhsValue.Value) + Convert.ToInt64(rhsValue.Value)), typeof(T)),
                    ulong _ => (T)Convert.ChangeType((Convert.ToUInt64(lhsValue.Value) + Convert.ToUInt64(rhsValue.Value)), typeof(T)),
                    int _ => (T)Convert.ChangeType((Convert.ToInt32(lhsValue.Value) + Convert.ToInt32(rhsValue.Value)), typeof(T)),
                    uint _ => (T)Convert.ChangeType((Convert.ToUInt32(lhsValue.Value) + Convert.ToUInt32(rhsValue.Value)), typeof(T)),
                    short _ => (T)Convert.ChangeType((Convert.ToInt16(lhsValue.Value) + Convert.ToInt16(rhsValue.Value)), typeof(T)),
                    ushort _ => (T)Convert.ChangeType((Convert.ToIntU16(lhsValue.Value) + Convert.ToUInt16(rhsValue.Value)), typeof(T)),
                    decimal _ => (T)Convert.ChangeType((Convert.ToDecimal(lhsValue.Value) + Convert.ToDecimal(rhsValue.Value)), typeof(T)),
                    double _ => (T)Convert.ChangeType((Convert.ToDouble(lhsValue.Value) + Convert.ToDouble(rhsValue.Value)), typeof(T)),
                    float _ => (T)Convert.ChangeType((Convert.ToSingle(lhsValue.Value) + Convert.ToSingle(rhsValue.Value)), typeof(T)),
                    char _ => (T)Convert.ChangeType((Convert.ToChar(lhsValue.Value) + Convert.ToChar(rhsValue.Value)), typeof(T)),
                    byte _ => (T)Convert.ChangeType((Convert.ToByte(lhsValue.Value) + Convert.ToByte(rhsValue.Value)), typeof(T)),
                    string _ => (T)Convert.ChangeType((Convert.ToString(lhsValue.Value) + Convert.ToString(rhsValue.Value)), typeof(T)),
                    _ => throw new InvalidOperationException($"Type {typeof(T).Name} is not appendable. Consider implementing the IAppendable interface.")
                }
            }
        };
    }

    // Mconcat
    public static Try<T> Mconcat<T>(this IEnumerable<Try<T>> ms)
    {
        return () =>
        {
            var value = ms.Head();

            foreach (var m in ms.Tail())
            {
                value = value.Mappend(m);
            }
            return value();
        };
    }

    // Pattern matching
    public static Func<R> Match<T, R>(this Try<T> self, Func<T, R> Success, Func<Exception, R> Fail)
    {
        if (Success == null) throw new ArgumentNullException("Success");
        if (Fail == null) throw new ArgumentNullException("Fail");

        return () =>
        {
            var res = self.Try();
            return res.IsFaulted
                ? Fail(res.Exception)
                : Success(res.Value);
        };
    }
    
    public static Func<R> Match<T, R>(this Try<T> self, Func<T, R> Success)
    {
        if (Success == null) throw new ArgumentNullException("Success");

        return () =>
        {
            var res = self.Try();
            return res.IsFaulted
                ? default(R)
                : Success(res.Value);
        };
    }
    
    public static Func<Unit> Match<T>(this Try<T> self, Action<T> Success, Action<Exception> Fail)
    {
        if (Success == null) throw new ArgumentNullException("Success");
        if (Fail == null) throw new ArgumentNullException("Fail");

        return () =>
        {
            var res = self.Try();

            if (res.IsFaulted)
                Fail(res.Exception);
            else
                Success(res.Value);

            return Unit.Default;
        };
    }
    
    public static Func<Unit> Match<T>(this Try<T> self, Action<T> Success)
    {
        if (Success == null) throw new ArgumentNullException("Success");

        return () =>
        {
            var res = self.Try();
            if (!res.IsFaulted)
                Success(res.Value);
            return Unit.Default;
        };
    }

    // Fetch and memoize the result
    public static Func<TryResult<T>> TryMemorize<T>(this Try<T> self)
    {
        TryResult<T> res;
        try
        {
            res = self();
        }
        catch (Exception e)
        {
            res = new TryResult<T>(e);
        }
        return () => res;
    }
}

public class Try
{
    // Mempty
    public static Try<T> Mempty<T>()
    {
        return () => default(T);
    }
}
```

# Reader
```csharp
// Allows for an 'environment' value to be carried through bind functions
public delegate A Reader<E, A>(E environment);

public static class Reader
{
    public static Reader<E, A> Return<E, A>(A value = default(A))
    {
        return (E env) => value;
    }

    public static Reader<E, E> Ask<E>(Func<E, E> f)
    {
        if (f == null) throw new ArgumentNullException("f");
        return (E env) => f(env);
    }

    public static Reader<E, E> Ask<E>()
    {
        return (E env) => env;
    }
}

// Reader monad extensions
public static class ReaderExt
{
    public static Reader<E, E> Ask<E, T>(this Reader<E, T> self, Func<E, E> f)
    {
        if (f == null) throw new ArgumentNullException("f");
        return (E env) => f(env);
    }

    public static Reader<E, E> Ask<E, T>(this Reader<E, T> self)
    {
        return (E env) => env;
    }

    public static Reader<E, U> Select<E, T, U>(this Reader<E, T> self, Func<T, U> select)
    {
        if (select == null) throw new ArgumentNullException("select");
        return (E env) => select(self(env));
    }

    public static Reader<E, V> SelectMany<E, T, U, V>(
        this Reader<E, T> self,
        Func<T, Reader<E, U>> bind,
        Func<T, U, V> project
        )
    {
        if (bind == null) throw new ArgumentNullException("bind");
        if (project == null) throw new ArgumentNullException("project");
        return (E env) =>
            {
                var resT = self(env);
                var resU = bind(resT);
                var resV = project(resT, resU(env));
                return resV;
            };
    }

    public static Func<T> Memorize<E, T>(this Reader<E, T> self, E environment)
    {
        var res = self(environment);
        return () => res;
    }

}
```

# Writer
```csharp
public delegate WriterResult<W, A> Writer<W, A>();

public struct WriterResult<W, A>
{
    public readonly A Value;
    public readonly IEnumerable<W> Output;

    internal WriterResult(A value, IEnumerable<W> output)
    {
        if (output == null) throw new ArgumentNullException("output");
        Value = value;
        Output = output;
    }
}

public static class WriterResult
{
    public static WriterResult<W, A> Create<W, A>(A value, IEnumerable<W> output)
    {
        if (output == null) throw new ArgumentNullException("output");
        return new WriterResult<W, A>(value, output);
    }
}

public static class Writer
{
    public static Writer<W, A> Return<W, A>(A a)
    {
        return () => WriterResult.Create<W, A>(a, new W[0]);
    }

    public static WriterResult<W, A> Tell<W, A>(A a, W w)
    {
        return WriterResult.Create<W, A>(a, new W[1] { w });
    }

    public static WriterResult<W, A> Tell<W, A>(A a, IEnumerable<W> ws)
    {
        if (ws == null) throw new ArgumentNullException("ws");
        return WriterResult.Create<W, A>(a, ws);
    }

    public static Writer<W, Unit> Tell<W>(W value)
    {
        return () => WriterResult.Create<W, Unit>(Unit.Default, new W[1] { value });
    }
}

public static class WriterExt
{
    /// <summary>
    /// Select
    /// </summary>
    public static Writer<W, U> Select<W, T, U>(this Writer<W, T> self, Func<T, U> select)
    {
        if (select == null) throw new ArgumentNullException("select");
        return () =>
        {
            var resT = self();
            var resU = select(resT.Value);
            return WriterResult.Create<W, U>(resU, resT.Output);
        };
    }

    public static Writer<W, V> SelectMany<W, T, U, V>(
        this Writer<W, T> self,
        Func<T, Writer<W, U>> bind,
        Func<T, U, V> project
    )
    {
        if (bind == null) throw new ArgumentNullException("bind");
        if (project == null) throw new ArgumentNullException("project");

        return () =>
        {
            var resT = self();
            var resU = bind(resT.Value).Invoke();
            var resV = project(resT.Value, resU.Value);

            return WriterResult.Create<W, V>(resV, resT.Output.Concat(resU.Output));
        };
    }

    public static Func<WriterResult<W, T>> Memorize<W, T>(this Writer<W, T> self)
    {
        var res = self();
        return () => res;
    }
}
```

# State
```csharp
public delegate StateResult<S, A> State<S, A>(S state);

public static class State
{
    public static State<S, A> Return<S, A>(A value = default(A))
    {
        return (S state) => new StateResult<S, A>(state, value);
    }

    public static State<S, S> Get<S>(Func<S, S> f)
    {
        if (f == null) throw new ArgumentNullException("f");
        return (S state) => StateResult.Create<S, S>(state, f(state));
    }

    public static State<S, S> Get<S>()
    {
        return (S state) => StateResult.Create<S, S>(state, state);
    }

    public static State<S, Unit> Put<S>(S state)
    {
        return _ => StateResult.Create<S, Unit>(state, Unit.Default);
    }
}

public struct StateResult<S, A>
{
    public readonly A Value;
    public readonly S State;

    internal StateResult(S state, A value)
    {
        Value = value;
        State = state;
    }
}

public static class StateResult
{
    public static StateResult<S, A> Create<S, A>(S state, A value)
    {
        return new StateResult<S, A>(state, value);
    }
}

public static class StateExt
{
    public static State<S, A> With<S, A>(this State<S, A> self, Func<S, S> f)
    {
        if (f == null) throw new ArgumentNullException("f");
        return (S state) =>
        {
            var res = self(state);
            return StateResult.Create<S, A>(f(res.State), res.Value);
        };
    }

    public static State<S, U> Select<S, T, U>(this State<S, T> self, Func<T, U> map)
    {
        if (map == null) throw new ArgumentNullException("map");
        return (S state) =>
        {
            var resT = self(state);
            return StateResult.Create<S, U>(resT.State, map(resT.Value));
        };
    }

    public static State<S, V> SelectMany<S, T, U, V>(
        this State<S, T> self,
        Func<T, State<S, U>> bind,
        Func<T, U, V> project 
        )
    {
        if (bind == null) throw new ArgumentNullException("bind");
        if (project == null) throw new ArgumentNullException("project");

        return (S state) =>
        {
            var resT = self(state);
            var resU = bind(resT.Value)(resT.State);
            var resV = project(resT.Value, resU.Value);
            return new StateResult<S, V>(resU.State, resV);
        };
    }

    public static Func<StateResult<S, A>> Memorize<S, A>(this State<S, A> self, S state)
    {
        var res = self(state);
        return () => res;
    }
}
```

# RWS
```csharp
public delegate RWSResult<W, S, A> RWS<R, W, S, A>(R r, S s);

public struct RWSResult<W, S, A>
{
    public readonly A Value;
    public readonly IEnumerable<W> Output;
    public readonly S State;

    internal RWSResult(A value, IEnumerable<W> output, S state)
    {
        Value = value;
        Output = output;
        State = state;
    }
}

public static class RWSResult
{
    public static RWSResult<W, S, A> Create<W, S, A>(A value, IEnumerable<W> output, S state)
    {
        if (output == null) throw new ArgumentNullException("output");
        return new RWSResult<W, S, A>(value, output, state);
    }
}

public static class RWS
{
    public static RWS<R, W, S, A> Return<R, W, S, A>(A a)
    {
        return (R r, S s) => RWSResult.Create<W, S, A>(a, new W[0], s);
    }

    public static RWSResult<W, S, A> Tell<W, S, A>(A a, W w)
    {
        return RWSResult.Create<W, S, A>(a, new W[1] { w }, default(S));
    }

    public static RWSResult<W, S, A> Tell<W, S, A>(A a, IEnumerable<W> ws)
    {
        if (ws == null) throw new ArgumentNullException("ws");
        return RWSResult.Create<W, S, A>(a, ws, default(S));
    }

    public static RWS<R, W, S, Unit> Tell<R, W, S>(W value)
    {
        return (R r, S s) => RWSResult.Create<W, S, Unit>(Unit.Default, new W[1] { value }, s);
    }

    public static RWS<R, W, S, R> Ask<R, W, S>(Func<R, R> f)
    {
        if (f == null) throw new ArgumentNullException("f");
        return (R r, S s) => RWSResult.Create(f(r), new W[0], s);
    }

    public static RWS<R, W, S, R> Ask<R, W, S>()
    {
        return (R r, S s) => RWSResult.Create(r, new W[0], s);
    }

    public static RWS<R, W, S, S> Get<R, W, S>(Func<S, S> f)
    {
        if (f == null) throw new ArgumentNullException("f");
        return (R r, S s) => RWSResult.Create<W, S, S>(s, new W[0], f(s));
    }

    public static RWS<R, W, S, S> Get<R, W, S>()
    {
        return (R r, S s) => RWSResult.Create<W, S, S>(s, new W[0], s);
    }

    public static RWS<R, W, S, Unit> Put<R, W, S>(S state)
    {
        return (R r, S s) => RWSResult.Create<W, S, Unit>(Unit.Default, new W[0], state);
    }

}

public static class RWSExt
{
    public static RWS<R, W, S, R> Ask<R, W, S, T>(this RWS<R, W, S, T> self, Func<R, R> f)
    {
        if (f == null) throw new ArgumentNullException("f");
        return (R r, S s) => RWSResult.Create(f(r), new W[0], s);
    }

    public static RWS<R, W, S, R> Ask<R, W, S, T>(this RWS<R, W, S, T> self)
    {
        return (R r, S s) => RWSResult.Create(r, new W[0], s);
    }

    public static RWS<R, W, S, U> Select<R, W, S, T, U>(this RWS<R, W, S, T> self, Func<T, U> select)
        where S : class
    {
        if (select == null) throw new ArgumentNullException("select");
        return (R r, S s) =>
        {
            var resT = self(r, s);
            var resU = select(resT.Value);
            return RWSResult.Create<W, S, U>(resU, resT.Output, resT.State ?? s);
        };
    }

    public static RWS<R, W, S, V> SelectMany<R, W, S, T, U, V>(
        this RWS<R, W, S, T> self,
        Func<T, RWS<R, W, S, U>> bind,
        Func<T, U, V> project
    )
        where S : class
    {
        if (bind == null) throw new ArgumentNullException("bind");
        if (project == null) throw new ArgumentNullException("project");

        return (R r, S s) =>
        {
            var resT = self(r, s);
            var resU = bind(resT.Value).Invoke(r, resT.State ?? s);
            var resV = project(resT.Value, resU.Value);

            return RWSResult.Create<W, S, V>(resV, resT.Output.Concat(resU.Output), resU.State ?? resT.State ?? s);
        };
    }

    public static Func<RWSResult<W, S, T>> Memorize<R, W, S, T>(this RWS<R, W, S, T> self, R r, S s)
    {
        var res = self(r, s);
        return () => res;
    }
}
```