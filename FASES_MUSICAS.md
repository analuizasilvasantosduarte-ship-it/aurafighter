# Aura Fighter — Músicas e BPM das Fases

| Fase | Música | Inimigo | BPM |
|------|--------|---------|-----|
| Tutorial | IT'S TV TIME (Forró) | — | 148 |
| Fase 1 | Chicago (Michael Jackson) | — | 100 |
| Fase 2 | Carioca Girls (Demais) | cachorro caramelo | 125 |
| Fase 3 | Never Gonna Meow You Up | gato | 113 |
| Fase 4 | Troll Face Song | troll face | 106 |
| Fase 5 | Stuck Inside (FNAF, versão forró) | Homem do ruim | 110 |
| Fase 6 | Megalovania Pisadinha | sans | 150–160 (padrão 150) |
| Fase 7 | Laurinha Costa - Six Seven (DJ Cabello & DJ Tchouzen) | tralaleiro | 158 |
| Boss Final | Fermo Aura (DUPÊ) | passarinho (Angry Birds) | 142 |

## Status dos arquivos de áudio

| Fase | Arquivo em `Assets/Audio/Music/` | Duração | BPM na cena |
|------|-----------------------------------|---------|-------------|
| Tutorial | `Tutorial - Its TV Time (Forro).mp3` ✅ | 2:35 | 148 ✅ |
| Fase 1 | `Fase 1 - Chicago (Michael Jackson).mp3` ✅ | 1:27 | 100 ✅ |
| Fase 2 | `Fase 2 - Carioca Girls (Demais).mp3` ✅ | 2:42 | 125 ✅ |
| Fase 3 | `Fase 3 - Never Gonna Meow You Up.mp3` ✅ | 1:01 | 113 ✅ |
| Fase 4 | `Fase 4 - Troll Face Song.mp3` ✅ | 2:42 | 106 ✅ |
| Fase 5 | `Fase 5 - Stuck Inside (FNAF forro).mp3` ✅ | 3:23 | 110 ✅ |
| Fase 6 | `Fase 6 - Megalovania Pisadinha.mp3` ✅ | 2:09 | 150 ✅ |
| Fase 7 | `Fase 7 - Laurinha Costa - Six Seven.mp3` ✅ | 1:38 | 158 ✅ |
| Boss Final | `Boss Final - Fermo Aura.mp3` ✅ | 3:21 | 142 ✅ |

✅ Tudo convertido, importado e ligado nas cenas.

> Áudios entram como **MP3** (o Unity não importa WebM/Opus). Conversão feita com
> `ffmpeg -i entrada.webm -vn -c:a libmp3lame -b:a 192k saida.mp3`.
> Importação configurada como *Compressed In Memory* (boa compressão sem travar o ritmo).

## Como aplicar no Unity

1. Coloque os arquivos de áudio em `Assets/Audio/Music/`
2. Menu **Aura Fighter → Aplicar BPM das Fases** (escreve o BPM em todas as cenas)
3. Em cada cena de fase, arraste a música no campo **Music Manager → Music Clip** (ou *theMusic* do GameManager)

## Onde o BPM age no jogo

- `BeatScroller.beatTempo` → velocidade das setas (`BPM / 60` unidades por segundo)
- `Conductor.songBpm` → relógio da música (usado em charts / judgement por tempo)
- Se `BeatScroller.unitsPerSecond > 0`, esse valor **ignora** o BPM (velocidade fixa). O menu zera esse campo pra o BPM mandar.

> Ao trocar a música, o BPM muda a velocidade da queda das setas: pode ser preciso reposicionar as setas da fase pra bater com a música.