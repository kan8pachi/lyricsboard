# How to define lyrics data

The definition rule like the supported format or the location of the files is subject to change in the future.

## Format

You need to define a lyrics data in LRC format for LyricsBoard.

Though LRC format generally has many dialects, we only supports the basic LRC formats like examples below.

### `time tagged lyrics` style
The time tag comes at the head of the line, and the sentence of the lyrics follows.

```
[00:13.70]すきすきチュッチュ
[00:16.65]ラブビーム
```

- Write time tag at the head of the line.
  - The time shows (minutes), (seconds), and (1/100 seconds) from the beginning of the song.
  - Enclose it in `[]`.
  - The delimiter is `:`. It can also be `.` only between (seconds) and (1/100 seconds).
  - Pad with 0 if it is less than 2 digits.
  - We have the feature to offset all lines across the board.
- The sentence of the lyrics follows.
  - Do not put any space after the time tag.


### `karaoke tagged lyrics` style
```
[00:13:70]す[00:14:25]き[00:14:46]す[00:14:81]き[00:15:13]チュッ[00:15:78]チュ
[00:16:65]ラ[00:16:90]ブ[00:17:15]ビー[00:17:66]ム
```

- The format is almost the same as the `time tagged lyrics` style.
- In addition to the head, you can write time tags between words or even in the middle of a word.
- The current version of LyricsBoard is able to read only the first time tag and ignores every other tags. Showing lyrics like subtitles in karaoke is in our future plan.


## File

### Location

The lyrics data will be loaded from this folder: `\steamapps\common\Beat Saber\UserData\LyricsBoard\lyrics`
It will be generated automatically if not exist, so it is a good idea to launch Beat Saber once right after you install LyricsBoard.

LyricsBoard can recognise the following two style of lyrics data.

- Just put the LRC data file alone.
  - The file name of LRC data should be: `<song-hash>.lrc`

- Place the LRC file and the metadata file in pairs.
  - The file name of LRC data should be: `<song-hash>.lrc`
  - The file name of metadata should be: `<song-hash>.json`

The encoding should be `UTF-8` and the newline character should be either `LF` or `CR+LF`.

### Identifying songs

The current version of LyricsBoard identify songs by `Song Hash`.
Since we know many players recognise songs by `BSR` and `Song Hash` might be inconvenient for them, this design would be changed in the future.

### metadata

After you created a LRC file, it is recommended to place it simply.
But you can also adjust the style of showing the lyrics data by placing a metadata file with the LRC file.

Metadata should be defined in JSON format like below.

```
{
  "OffsetMs": "1200",
  ...
}
```

The metadata has the following adjustment parameters. Define each entry only when you intend to change it from the default value.

- `OffsetMs` : Add this offset(ms) to the time tag in LRC file across the board. It is useful when you already have a LRC file but the timing of the song in game does not match. It often occurs when the mapper inserts silence time at the head of the song.
- `MaxExpirationMs` : The lyrics line disappears 10,000 ms later after it appeared by default. This parameter is generally useful for up tempo or slow tempo songs. This configuration would be changed especially when we implement karaoke mode.
- `AnimationDurationMs` : Define the animation speed when the lyrics line appears and disappears. It is 200 ms by default.
- `StandbyDurationMs` : The next line of the current lyrics appears 1,500 ms before it starts by default.
