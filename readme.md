# RvB.Linq
An alternative Linq implementation, built using ref structs instead of classes. It should provide better performance and memory characteristics than the native implementation of Linq

Enumerables can use this by converting them to Iterables by using the `.AsIterable<T>()` extension method.
