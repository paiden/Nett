# What is Nett
Nett is a library that helps to read and write [TOML](https://github.com/toml-lang/toml) files in .Net. 

# Objectives
+ Compatible (but not 100% compliant) with **latest** TOML spec. All currently available TOML libs for C# (.Net) are pre 0.1.0 spec 

# Differences to original TOML spec
* Nett also allows you to specify timespan values. Timespan currently isn't supported in the original 
TOML spec.

# Getting Started
[TODO]

# Changelog
No releases yet

# TODO / Roadmap
+ [0.2.0] Preserving processing of comments during reads / writes
+ [ND] Better error message in many cases
    +  `@a = [""C:\dir1""]`. Very weird error message because tokenizer fails, so parser hasn't chance to produce better message.
