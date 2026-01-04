# Как да намерите или създадете HDRP Asset

## Разлика между файловете:

### HDRenderPipelineGlobalSettings.asset (това което намерихте):
- Това е **Global Settings** за HDRP
- Използва се за настройки като Custom Post Process Orders
- **НЕ е** това което трябва да зададете в Graphics Settings!

### HDRenderPipelineAsset (това което ви трябва):
- Това е **Render Pipeline Asset**
- Това е файлът който трябва да зададете в **Project Settings > Graphics > Scriptable Render Pipeline Settings**
- Обикновено се нарича "HDRenderPipelineAsset" или "HDRP Asset"

## Как да намерите HDRP Asset:

### Стъпка 1: Потърсете в Project панела

1. В Project панела, използвайте **Search bar** (горният десен ъгъл)
2. Потърсете: `HDRenderPipelineAsset` или `HDRP Asset`
3. Проверете в папки като:
   - `Assets/Settings/`
   - `Assets/RenderPipelineAssets/`
   - `Assets/HDRPDefaultResources/`
   - Или в корена на `Assets/`

### Стъпка 2: Проверете Project Settings

1. Отворете **Edit > Project Settings > Graphics**
2. В **Scriptable Render Pipeline Settings**, проверете:
   - Ако има зададен asset, кликнете на него
   - Unity ще ви покаже къде се намира в Project панела
   - Ако е празно → трябва да създадете нов

### Стъпка 3: Създайте HDRP Asset (ако не го намирате)

1. В Project панела, изберете папката където искате да го създадете (например `Assets/Settings/`)
2. Десен бутон > **Create > Rendering > HDRP Asset**
3. Ако **НЕ виждате** тази опция:
   - Проверете дали HDRP package е инсталиран
   - Window > Package Manager
   - Потърсете "High Definition RP"
   - Ако не е инсталиран, инсталирайте го

4. Назовете го `HDRP_Asset` или `HDRenderPipelineAsset`

### Стъпка 4: Задайте HDRP Asset в Graphics Settings

1. Отворете **Edit > Project Settings > Graphics**
2. В **Scriptable Render Pipeline Settings**:
   - Кликнете на малкото **кръгче** (обект picker) до полето
   - Изберете вашия HDRP Asset (създадения или намерения)
3. Затворете Project Settings

### Стъпка 5: Рестартирайте Unity

1. Затворете Unity Editor напълно
2. Отворете го отново
3. Проверете Volume Profile - вече не трябва да виждате "No SRP in use"

## Как да разпознаете HDRP Asset:

### В Project панела:
- Иконата е **синя кубче** (без оранжева скоба)
- Името обикновено е "HDRenderPipelineAsset" или "HDRP Asset"
- Типът в Inspector е "HDRenderPipelineAsset"

### В Inspector (когато го изберете):
- Виждате секции като "Rendering", "Lighting", "Post-processing", и т.н.
- Има много настройки за HDRP

## Ако не можете да създадете HDRP Asset:

### Проверете дали HDRP е инсталиран:

1. **Window > Package Manager**
2. Вляво, изберете **In Project** или **Unity Registry**
3. Потърсете "High Definition RP" или "HDRP"
4. Проверете версията - трябва да е инсталирана

### Ако HDRP не е инсталиран:

1. Window > Package Manager
2. Unity Registry (вляво)
3. Потърсете "High Definition RP"
4. Кликнете **Install**
5. След инсталация, Unity може автоматично да създаде HDRP Asset

## След като зададете HDRP Asset:

1. Рестартирайте Unity
2. Отворете "VHS Volume Profile"
3. Вече НЕ трябва да виждате "No SRP in use"
4. Кликнете "Add Override"
5. Трябва да виждате "Custom > Digital Glitch"!

## Важно:

- **HDRP Asset** (Render Pipeline Asset) ≠ **HDRP Global Settings**
- Трябва да зададете **HDRP Asset** в Graphics Settings
- Без зададен HDRP Asset, Unity показва "No SRP in use"

