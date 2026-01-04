# Как да създадете правилния HDRP Volume Profile

## Проблем:
Създадохте "Post Process Profile" (от стария Post-Processing Stack v2), но Digital Glitch работи САМО с HDRP Volume Profile.

## Решение - Стъпка по стъпка:

### Вариант 1: Създаване чрез GameObject (ПРЕПОРЪЧИТЕЛНО)

1. **Създайте празен GameObject:**
   - Hierarchy: десен бутон > Create Empty
   - Назовете го "Temp Volume Setup"

2. **Добавете HDRP Volume компонент:**
   - Изберете "Temp Volume Setup"
   - Inspector: Add Component
   - Потърсете и добавете: **Volume** (от HDRP, НЕ Post-process Volume!)

3. **Създайте Volume Profile:**
   - В Inspector, на Volume компонента
   - В полето **Profile**, кликнете на малкия **кръгче** (обект picker) до полето
   - В прозореца, кликнете **Create** бутона в долния ляв ъгъл
   - Назовете го `VHS_Profile_HDRP`
   - Това създава **правилния HDRP Volume Profile**!

4. **Добавете Digital Glitch:**
   - Отворете `VHS_Profile_HDRP` (двойно кликване)
   - В Inspector, кликнете **Add Override** или **+**
   - Изберете **Custom > Digital Glitch**

5. **Приложете към вашия Volume:**
   - Изберете "VHS Post Process Volume" GameObject
   - Ако няма Volume компонент (HDRP), добавете го
   - В полето **Profile**, дръпнете `VHS_Profile_HDRP`

6. **Изтрийте временния GameObject:**
   - Изтрийте "Temp Volume Setup" (вече не е нужен)

### Вариант 2: Директно създаване (ако работи)

1. В Project панела, десен бутон в папката където искате да създадете
2. **ВАЖНО:** НЕ използвайте "Create > Post Process Profile"
3. Вместо това:
   - Създайте GameObject с Volume компонент (като в Вариант 1)
   - Или използвайте менюто: **Assets > Create > Volume Profile** (ако го има)

## Как да разпознаете правилния тип:

### ❌ НЕПРАВИЛНО - Post Process Profile (стария):
- Иконата е синя с "PP"
- В Inspector пише "Post Process Profile"
- НЕ работи с HDRP custom post-process ефекти

### ✅ ПРАВИЛНО - Volume Profile (HDRP):
- Иконата е синя с "V" или подобна
- В Inspector пише "Volume Profile" или "HDRP Volume Profile"
- Работи с Digital Glitch!

## Ако все още не виждате Digital Glitch в менюто:

1. **Рестартирайте Unity Editor** (най-важно!)
2. Проверете Console за грешки
3. Проверете дали скриптовете са компилирани:
   - `DigitalGlitch.cs` - трябва да няма грешки
   - `DigitalGlitchRenderer.cs` - трябва да няма грешки
4. Проверете Project Settings:
   - Edit > Project Settings > Graphics > HDRP Global Settings
   - Custom Post Process Orders > After Post Process
   - Трябва да виждате `DigitalGlitchRenderer`

## Алтернативно решение - Проверка на типа:

Ако не сте сигурни какъв тип е вашия профил:

1. Изберете профила в Project панела
2. В Inspector, проверете типа в горната част
3. Ако пише "Post Process Profile" - това е грешният тип
4. Ако пише "Volume Profile" - това е правилният тип

## Важно:

- **НЕ можете** да конвертирате Post Process Profile към Volume Profile
- Трябва да създадете **НОВ** Volume Profile
- Можете да копирате настройките от стария профил ръчно

