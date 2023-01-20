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
[00:02:96]3[00:03:36].[00:03:76]1[00:04:16]4[00:04:56]1[00:04:96]5[00:05:36]9[00:05:76]2
[00:06:16]6[00:06:56]5[00:06:96]3[00:07:36]5[00:07:76]8[00:08:16]9[00:08:56]7[00:08:96]9
```

- The format is almost the same as the `time tagged lyrics` style.
- In addition to the head, you can write time tags between words or even in the middle of a word.

## File

### Location

The lyrics data will be loaded from this folder: `\steamapps\common\Beat Saber\UserData\LyricsBoard\lyrics`
It will be generated automatically if not exist, so it is a good idea to launch Beat Saber once right after you install LyricsBoard.
You can create any number of sub-folders in this folder.
It might be easier to manage the lyrics if you create the folder with the same name as those in `CustomLevels`.

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

### Metadata

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
- `MaxExpirationMs` : The lyrics line disappears 10,000 ms later after it appeared by default. This parameter is generally useful for up tempo or slow tempo songs.
- `AnimationDurationMs` : Define the animation speed when the lyrics line appears and disappears. It is 200 ms by default.
- `StandbyDurationMs` : The next line of the current lyrics appears 1,500 ms before it starts by default.

### Making LRC file

LRC file is just a text data so you can use your favourite text editor to create it.
However, making `time tagged` and `karaoke tagged` data is so time consuming, we reccomend looking for an editor dedicated to LRC data.
