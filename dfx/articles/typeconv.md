
# Mapping with default settings

| .Net                          | TOML                                   |
|-------------------------------|----------------------------------------|
| `char X = 1`                  | `X = 1`                                |
| `short X = 1`                 | `X = 1`                                |
| `int X = 1`                   | `X = 1 `                               |
| `long X = 1`                  | `X = 1`                                |
| `float X = 1`                 | `X = 1.0`                              |
| `double X = 1`                | `X = 1.0`                              |
| `string X = "a"`              | `X = "a"`                              |
| `string X = "a\r\nb"`         | `X = """a\b"""`                        |
| `char[] X  = {1, 2}`          | `X = [1, 2]`                           |
| `short[] X = {1, 2}`          | `X = [1, 2]`                           |
| `int[] X = {1, 2}`            | `X = [1, 2]`                           |
| `long[] X = {1, 2}`           | `X = [1, 2]`                           |
| `float[] X = {1, 2}`          | `X = [1.0, 2.0]`                       |
| `double[] X = {1, 2}`         | `X = [1.0, 2.0]`                       |
| `string[] X = {"a", "b"}`     | `X = ["a", "b"] `                      |
| `char[][] X  = [{1}][{2}]`    | `X = [[1], [2]]`                       |
| `short[][] X = [{1}][{2}]`    | `X = [[1], [2]]`                       |
| `int[][] X = [{1}][{2}]`      | `X = [[1], [2]]`                       |
| `long[][] X = [{1}][{2}]`     | `X = [[1], [2]]`                       |
| `float[][] X = [{1}][{2}]`    | `X = [[1.0], [2.0]]`                   |
| `double[][] X = [{1}][{2}]`   | `X = [[1.0], [2.0]]`                   |
| `DateTimeOffset X = ...`      | `X = 1979-05-27 00:32:00.999999-07:00` |
| `DateTime X = ...`            | `X = 1979-05-27 00:32:00.999999`       |
| `MyClass X = new MyClass()`   | `[X]`                                  |
| `MyClass[] X = new MyClass[]` | `[[X]]`                                |
| `int[,] X = {{1}, {2}}`       | Not supported                          |
| `decimal X = 1`               | Not supported                          |
| `Guid X = Guid.Empty`         | Not Supported                          |
| `Enum X = Enum.Value`         | Not Supported                          |


