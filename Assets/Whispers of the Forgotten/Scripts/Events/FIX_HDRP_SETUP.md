# Проблем: "No SRP in use" - HDRP не е активиран!

## Проблемът:
В Inspector на Volume Profile виждате **"No SRP in use"**. Това означава, че Unity не разпознава HDRP като активен render pipeline.

**Без активен HDRP, Digital Glitch ефектът НЕ може да работи!**

## Решение - Активирайте HDRP:

### Стъпка 1: Проверете Graphics Settings

1. Отворете **Edit > Project Settings**
2. Вляво, изберете **Graphics**
3. В секцията **Scriptable Render Pipeline Settings**, проверете:
   - Трябва да има зададен **HDRP Asset** (например "HDRenderPipelineAsset")
   - Ако полето е празно → това е проблемът!

### Стъпка 2: Намерете или създайте HDRP Asset

**Ако НЕ виждате HDRP Asset в Graphics Settings:**

1. В Project панела, потърсете за файлове с име:
   - `HDRenderPipelineAsset`
   - `HDRP Asset`
   - Или файлове с разширение `.asset` в папки като "Settings", "RenderPipelineAssets", и т.н.

2. Ако НЕ намирате HDRP Asset:
   - В Project панела, десен бутон в папката където искате да го създадете
   - **Create > Rendering > HDRP Asset** (или подобно)
   - Назовете го `HDRP_Asset`

3. Ако НЕ виждате опцията "HDRP Asset" в менюто:
   - Проверете дали HDRP package е инсталиран
   - Window > Package Manager
   - Потърсете "High Definition RP" или "HDRP"
   - Ако не е инсталиран, инсталирайте го

### Стъпка 3: Задайте HDRP Asset в Graphics Settings

1. Отворете **Edit > Project Settings > Graphics**
2. В **Scriptable Render Pipeline Settings**, кликнете на малкото кръгче (обект picker)
3. Изберете вашия HDRP Asset (или създадения `HDRP_Asset`)
4. Затворете Project Settings

### Стъпка 4: Проверете Quality Settings (опционално)

1. Отворете **Edit > Project Settings > Quality**
2. За всяко качество (Very Low, Low, Medium, High, и т.н.):
   - Проверете дали **Render Pipeline Asset** е зададен на същия HDRP Asset
   - Ако не е, задайте го

### Стъпка 5: Рестартирайте Unity

1. Затворете Unity Editor напълно
2. Отворете го отново
3. Проверете дали "No SRP in use" вече не се показва

### Стъпка 6: Проверете отново Volume Profile

1. Отворете "VHS Volume Profile"
2. В Inspector, вече НЕ трябва да виждате "No SRP in use"
3. Вместо това, трябва да виждате списък с налични ефекти
4. Кликнете "Add Override"
5. Трябва да виждате "Custom > Digital Glitch" в менюто!

## Как да разпознаете че HDRP е активен:

### ✅ Правилно (HDRP е активен):
- В Inspector на Volume Profile, виждате списък с ефекти
- Можете да добавяте HDRP ефекти (Bloom, Vignette, и т.н.)
- Виждате "Custom > Digital Glitch" в менюто

### ❌ Неправилно (HDRP не е активен):
- В Inspector на Volume Profile, виждате "No SRP in use"
- Не можете да добавяте ефекти
- Digital Glitch не се появява в менюто

## Ако не можете да намерите HDRP Asset:

### Проверете дали HDRP е инсталиран:

1. **Window > Package Manager**
2. Вляво, изберете **In Project** или **Unity Registry**
3. Потърсете "High Definition RP" или "HDRP"
4. Ако го виждате като инсталиран → HDRP Asset трябва да съществува някъде
5. Ако НЕ го виждате → трябва да го инсталирате първо

### Инсталиране на HDRP (ако не е инсталиран):

1. Window > Package Manager
2. Unity Registry (вляво)
3. Потърсете "High Definition RP"
4. Кликнете **Install**
5. След инсталация, Unity автоматично ще създаде HDRP Asset

## Важно:

- **БЕЗ активен HDRP, Digital Glitch НЕ може да работи!**
- Трябва да активирате HDRP преди да добавите custom post-process ефекти
- След активиране на HDRP, рестартирайте Unity

## След като активирате HDRP:

1. Отворете "VHS Volume Profile"
2. Кликнете "Add Override"
3. Трябва да виждате "Custom > Digital Glitch"
4. Добавете го и настройте параметрите!

