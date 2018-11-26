# Differences to original TOML spec
'Nett' also allows you to specify duration values. Duration currently isn't supported in the original
'TOML' spec. A Go-like duration format is supported (units have to be ordered large to small
and can occur 1 time at most).
Examples:

+ 2_500d
+ 0.5d2h3m
+ 1d2h3m4s5ms
+ -2h30m
+ 400m2000s