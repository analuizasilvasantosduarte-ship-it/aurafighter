# Aura Fighter — Design System v1.0

Sistema de design para o jogo **Aura Fighter** (Unity 2D, URP, pixel art).

> **O que é:** um conjunto único e consistente de cores, tipografia, espaçamentos e componentes
> para todas as telas do jogo (menu, cutscenes, gameplay, resultados e derrota).

---

## 1. Visão geral & linguagem visual

| Diretriz | Valor |
|---|---|
| Gênero | Ritmo + luta, estilo arcade |
| Estética | Pixel art (8–16 bit), bordas grossas, contraste alto |
| Fonte principal | **Thaleah Fat** (`Assets/Thaleah_PixelFont/`, TTF + PNG sprite) |
| Fonte fallback (UI dinâmica) | Arial / `LegacyRuntime.ttf` (usada em UI criada via código) |
| Resolução de referência | **1920 × 1080** |
| Canvas Scaler | `ScaleWithScreenSize`, `matchWidthOrHeight = 0.5` |
| Base de espaçamento | **4 px** |

**Narrativa visual:** o jogador é o **Carlos** (verde/laranja) defendendo sua "Aura" (energia dourada/)
contra o chefe **Red Bird** (vermelho/roxo). A paleta reflete esse confronto: tons **quentes dourados/ember**
(protagonista e energia) contra tons **roxos profundos** (inimigo), com **neón de 4 pistas** de ritmo
(azul/amarela/vermelha/verde).

---

## 2. Tokens de cor

### 2.1 Núcleo da marca

| Token | Hex | Uso |
|---|---|---|
| `Aura/Gold` | `#FFD200` | Energia, botão amarelo, pista 2, rank S |
| `Aura/GoldText` | `#FFD933` | Texto de destaque: Score, nome do speaker |
| `Aura/Violet` | `#4F119A` | Aura mística, plumagem do Red Bird, gradientes |
| `Aura/DeepPurple` | `#2C004B` | Roxo profundo — background Red Bird, sombras |
| `Aura/Ember` | `#D93A22` | Vermelho da marca — logo/título, corpo do Red Bird |
| `Aura/EmberDark` | `#B6240E` | Ember em estado hover/ativo |
| `Aura/Lavender` | `#E0C7FF` | Brilho/glow, destaques do Red Bird |

### 2.2 Semânticos (jogo)

| Token | Hex | Uso |
|---|---|---|
| `Semantics/Success` | `#18BE03` | Julgamento **Perfect**, vida cheia, rank A |
| `Semantics/Good` | `#F1E603` | Julgamento **Good**, rank C |
| `Semantics/Neutral` | `#035AD9` | Julgamento **Normal**, informações |
| `Semantics/Danger` | `#E60101` | Julgamento **Miss**, ranking F |
| `Semantics/ButtonRed` | `#FF0000` | Botão vermelho, pista 3 |
| `Semantics/ButtonBlue` | `#003CFF` | Botão azul, pista 1 |
| `Semantics/ButtonGreen` | `#0E9000` | Botão verde, pista 4 |
| `Semantics/Defeat` | `#FF3333` | Título "DERROTA" (Unity `Color(1, 0.2, 0.2)`) |

### 2.3 Neutros

| Token | Hex | Uso |
|---|---|---|
| `Neutral/Black` | `#000000` | Fundos, contornos, logo |
| `Neutral/BlackBlue` | `#0F0F1F` | Fundo padrão de cutscene |
| `Neutral/Charcoal` | `#2C2C2C` | Borda externa dos botões |
| `Neutral/CharcoalLight` | `#3D3D3D` | Gradiente dos botões (down) |
| `Neutral/Gray` | `#555555` | Cinza médio (borda/recuo) |
| `Neutral/GrayLight` | `#909090` | Cinza claro (borda dos botões) |
| `Neutral/GrayLighter` | `#E0E0E0` | High-key de personagens/boss |
| `Neutral/White` | `#FFFFFF` | Texto principal, destaques |
| `Neutral/Overlay` | `rgba(0,0,0,0.85)` | Dimmer da tela de derrota |

### 2.4 Notas do ritmo (4 pistas)

| Pista | Cor básica | Efeito |
|---|---|---|
| 1 — Pista Azul | `#003CFF` | `#035AD9` |
| 2 — Pista Amarela | `#FFD200` | `#F1E603` |
| 3 — Pista Vermelha | `#FF0000` | `#E60101` |
| 4 — Pista Verde | `#0E9000` | `#18BE03` |

### 2.5 Ranks (tela de resultados)

| Ranking | % de acerto | Cor |
|---|---|---|
| **S** | > 95% | `#FFD200` |
| **A** | > 85% | `#18BE03` |
| **B** | > 70% | `#003CFF` |
| **C** | > 55% | `#F1E603` |
| **D** | > 40% | `#909090` |
| **F** | ≤ 40% | `#E60101` |

*(Lógica em `GameManager.cs`, método `EndRound`.)*

### 2.6 Ambientes por fase

Cores dominantes extraídas de `Assets/Sprites/backgrounds/*.png`.

| Fase (background) | Base | Destaque | Profundo/Sombra |
|---|---|---|---|
| Crepúsculo (`9.png`) | `#B66E07` | `#EC722F` | `#382402` |
| Rosa/pôr do sol (`10.png`) | `#996253` | `#C7697E` | `#241A16` |
| Azul profundo (`11.png`) | `#254F69` | `#006789` | `#0A1822` |
| Aqua claro (`12.png`) | `#418EA8` | `#3FA1BA` | `#191919` |
| Savana/lavanda (`13.png`) | `#B1A5C9` | `#866E6E` | — |

### 2.7 Personagens

| Elemento | Carlos (jogador) | Red Bird (chefe) |
|---|---|---|
| Corpo/plumagem | `#B8EA62` (camisa) | `#D93A22` / `#BF3108` |
| Pele/brilho | `#FDAE63` | `#F7E4DE` |
| Detalhe escuro | `#662806` (cabelo) / `#702902` | `#2C004B` |
| Acessórios | `#2C5B9C` (calça) | `#4F119A` / `#E0C7FF` |

---

## 3. Tipografia

### Fonte principal: **Thaleah Fat** (pixel)

| Token | Tamanho | Uso |
|---|---|---|
| `Type/DisplayXL` | **96** | Título de tela ("DERROTA") |
| `Type/DisplayL` | **64** | Título de fase / banners |
| `Type/H1` | **48** | Pontuação, ranking final |
| `Type/H2` | **36** | Subtítulos / avisos |
| `Type/BodyL` | **34** | Mensagem de diálogo |
| `Type/BodyM` | **28** | Nome do speaker, prompts ("Pressione Espaço ▼") |
| `Type/Label` | **24** | Dica de continuação / rótulos pequenos |

### Fonte fallback (código)

Toda UI criada via código usa `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`.
Regras de uso:
- Alinhamento padrão: `MiddleCenter` para títulos, `UpperLeft` para diálogo.
- `FontStyle.Bold` para destaque (título/score), `Italic` para hints.
- Cores: texto principal `Neutral/White`, destaque `Aura/GoldText`, erro `Semantics/Defeat`.

---

## 4. Espaçamento e grade

Base de **4 px**: `4 · 8 · 12 · 16 · 24 · 32 · 48 · 64 · 80 · 120`

| Token | Valor | Exemplo de uso |
|---|---|---|
| `Space/2` | 8 | Gap entre itens de HUD |
| `Space/4` | 16 | Padding interno de botões/cards |
| `Space/6` | 24 | Gap do layout de resultados |
| `Space/8` | 32 | Raio/margem de modais |
| `Space/10` | 48 | Margem superior (HUD) |
| `Space/14` | 80 | Margem lateral segura (painéis) |
| `Space/20` | 120 | Distância entre seções |

---

## 5. Layout de tela (1920 × 1080)

```
┌──────────────────────────────────────────────────────┐
│  [Score]  [Multiplier]        [HP]  [Aura]    ← HUD topo (80px) │
│                                                      │
│                  (linha de julgamento)               │
│                                                      │
│  ┌────────────────────────────────────────────┐      │
│  │  Painel de diálogo (altura 330, base 170)  │      │
│  └────────────────────────────────────────────┘      │
└──────────────────────────────────────────────────────┘
   margens laterais: 80 px
```

- **HUD (topo):** Score (esquerda), Multiplicador, Vida (HP) e Aura — ocupam no máximo a faixa superior de 160 px.
- **Área do jogo:** pistas de notas descem até a **linha de julgamento** (centro vertical).
- **Painel de diálogo:** ancorado na base, `anchoredPosition (0, 170)`, `sizeDelta (-80, 330)` (igual ao `DialogueSystem.cs`).

---

## 6. Componentes

### 6.1 Botões — 4 cores × 3 estados

Sprites em `Assets/Graphics/` (250×250) e `Assets/Graphics/botões/` (hover).

| Variante | Normal | Pressed | Hover |
|---|---|---|---|
| Azul | `Buttons_Blue.png` | `Buttons_Blue_Pressed.png` | `Botão Hover 1.png` |
| Amarela | `Buttons_Yellow.png` | `Buttons_Yellow_Pressed.png` | `Botão Hover 2.png` |
| Vermelha | `Buttons_Red.png` | `Buttons_Red_Pressed.png` | `Botão Hover 3.png` |
| Verde | `Buttons_Green.png` | `Buttons_Green_Pressed.png` | `Botão Hover 4.png` |

- **Estilo:** cor de preenchimento + borda externa `Charcoal` `#2C2C2C` + brilho `#909090`/`#555555`.
- Estados desabilitados usam cinza (`LevelButton.requiredPoints` não atingido).

### 6.2 Painel de diálogo

- Sprite de fundo: caixa em `Assets/Sprites/Personagem principal/Caixa de dialago/`.
- Fallback (sem sprite): `Overlay` `rgba(0,0,0,0.85)` com bordas arredondadas.
- Layout interno: **Speaker** (`BodyM`, bold, `Aura/GoldText`) no topo; **Mensagem** (`BodyL`, branco) abaixo; **"Pressione Espaço ▼"** (Label, itálico, piscando 0.45s/0.45s) no rodapé.

### 6.3 HUD de gameplay

| Elemento | Fonte/Cor | Conteúdo |
|---|---|---|
| Score | `H1`, `Aura/GoldText` | `Score: <valor>` |
| Multiplicador | `BodyM`, branco | `Multiplier: x<N>` |
| Vida (HP) | `BodyM`, branco | `120/100` — cor muda p/ `Semantics/Danger` abaixo de 25% |
| Aura | `BodyM`, `Aura/GoldText` | `Aura: <total>` |

*(Opções de acerto: Normal 100, Good 125, Perfect 150 — × multiplicador 1→2→4→6→8.)*

### 6.4 Notas

- 4 pistas, cores em **2.4**.
- Teclas: definidas por `NoteObject.keyToPress` (uma tecla por pista).
- Precisão: `> 0.25` = Normal, `> 0.05` = Good, `≤ 0.05` = Perfect (distância da linha).

### 6.5 Julgamentos / efeitos

| Julgamento | Cor (effects.png) | Pontos base |
|---|---|---|
| Perfect | `#18BE03` | 150 |
| Good | `#F1E603` | 125 |
| Normal | `#035AD9` | 100 |
| Miss | `#E60101` | 0 (dano: HP `damagePerMiss = 5`) |

### 6.6 Tela de resultados

- Segundo plano completo + `Overlay`.
- **Percentual** de acerto (`BodyL`), contadores por julgamento (`BodyM`), **Rank** (`DisplayL`, cor de 2.5), **Pontuação final** (`H1`, `Aura/GoldText`).
- Avanço: "Pressione Espaço para continuar ▼" (a partir da cena `nextSceneName`).

### 6.7 Tela de derrota

- Título **"DERROTA"** — `DisplayXL` (96), bold, `Semantics/Defeat`.
- Subtítulo branco (`BodyM`), pontuação (`H1`, `Aura/GoldText`), prompt itálico.
- Trigger: `Vida.OnDeath` / `forceDefeat` (derrotado pelo Red Bird).

---

## 7. Implementação

### No Unity

Use a classe estática **`Assets/script/DesignTokens.cs`** (criada junto com este documento):

```csharp
GetComponent<Text>().color = DesignTokens.Colors.AuraGoldText;      // texto de destaque
DesignTokens.Colors.VidaBaixa                                       // vida crítica
DesignTokens.Ranks.Cor(Rank.S);                                     // cor do rank
DesignTokens.GetColor("#D93A22");                                   // hex direto (sem #)
```

Regras práticas:
1. **Nunca** hard-code cores novas — adicione ao `DesignTokens` e reutilize.
2. Texto "dinâmico" usa fallback Arial; UI construída em cena usa Thaleah Fat.
3. Respeite a base 4 px nos espaçamentos.

### No Figma

Os tokens estão no formato **DTCG/Tokens Studio** em `design-tokens.json`:

1. Instale o plugin **Tokens Studio for Figma** (figma.com/community).
2. *Tokens → Import → JSON* e selecione `design-tokens.json`.
3. Os estilos de cor/fonte viram **estilos do Figma** e podem ser aplicados nos frames.
4. Monte os componentes seguindo as seções 5 e 6 deste documento.

---

## 8. Checklist de consistência

- [ ] Botões usam os 3 estados (normal/pressed/hover) das 4 variantes.
- [ ] Score/Multiplicador/Aura no HUD superior, sempre visíveis.
- [ ] Notas com cores das 4 pistas (nunca fora da paleta).
- [ ] Painel de diálogo com margens de 80 px laterais e 170 px da base.
- [ ] Ranks com as cores definidas (S→F).
- [ ] "DERROTA" sempre com `Semantics/Defeat` no tamanho 96.
- [ ] Espaçamentos múltiplos de 4.