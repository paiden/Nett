namespace Nett.PerfTests
{
    internal static class TomlSource
    {
        public const string TomlV1 = @"

i = 1
f = 1.0
s1 = ""String1""
s2 = 'String2'
s3 = '''
String
3
'''
dt=1979-05-27T07:32:00-08:00
ts=01:01:01
ai = [+2, -3, 1_2_3_4_5]
af = [1.0, 1.1415, -0.01, 5e+22, -2E-2]
sa = [""String1"", 'String2', '''String3''']

[Subtable]
i = 1
f = 1.0
s1 = ""String1""
s2 = 'String2'
s3 = '''
String
3
'''
dt=1979-05-27T07:32:00-08:00
ts=01:01:01
ai = [+2, -3, 1_2_3_4_5]
af = [1.0, 1.1415, -0.01, 5e+22, -2E-2]
sa = [""String1"", 'String2', '''String3''']

[[products]]
name = ""Hammer""
sku = 738594937

[[products]]

[[products]]
name = ""Nail""
sku = 284758393
color = ""gray""

[[fruit]]
  name = ""apple""

  [fruit.physical]
        color = ""red""
    shape = ""round""

  [[fruit.variety]]
    name = ""red delicious""

  [[fruit.variety]]
    name = ""granny smith""

[[fruit]]
  name = ""banana""

  [[fruit.variety]]
    name = ""plantain""

";

        public const string JsonV1 = @"{
   ""i"":1,
   ""f"":1.0,
   ""s1"":""String1"",
   ""s2"":""String2"",
   ""s3"":""String\r\n3\r\n"",
   ""dt"":""1979-05-27T15:32:00Z"",
   ""ts"":""01:01:01"",
   ""ai"":[
      2,
      -3,
      12345
   ],
   ""af"":[
      1.0,
      1.1415,
      -0.01,
      5E+22,
      -0.02
   ],
   ""sa"":[
      ""String1"",
      ""String2"",
      ""String3""
   ],
   ""Subtable"":{
      ""i"":1,
      ""f"":1.0,
      ""s1"":""String1"",
      ""s2"":""String2"",
      ""s3"":""String\r\n3\r\n"",
      ""dt"":""1979-05-27T15:32:00Z"",
      ""ts"":""01:01:01"",
      ""ai"":[
         2,
         -3,
         12345
      ],
      ""af"":[
         1.0,
         1.1415,
         -0.01,
         5E+22,
         -0.02
      ],
      ""sa"":[
         ""String1"",
         ""String2"",
         ""String3""
      ],
      ""Subtable"":null,
      ""products"":null,
      ""fruit"":null
   },
   ""products"":[
      {
         ""name"":""Hammer"",
         ""sku"":738594937,
         ""color"":null
      },
      {
         ""name"":null,
         ""sku"":0,
         ""color"":null
      },
      {
         ""name"":""Nail"",
         ""sku"":284758393,
         ""color"":""gray""
      }
   ],
   ""fruit"":[
      {
         ""name"":""apple"",
         ""physical"":{
            ""color"":""red"",
            ""shape"":""round""
         },
         ""variety"":[
            {
               ""name"":""red delicious""
            },
            {
               ""name>"":""granny smith""
            }
         ]
      },
      {
         ""name"":""banana"",
         ""physical"":null,
         ""variety"":[
            {
               ""name"":""plantain""
            }
         ]
      }
   ]
}";
    }
}
