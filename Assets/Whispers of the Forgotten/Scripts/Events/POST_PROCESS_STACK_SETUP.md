# Настройка на Digital Glitch за Post-Processing Stack v2

## Важно:
Вашият проект използва **Post-Processing Stack v2** (Built-in Render Pipeline), не HDRP. Ето как да настроите Digital Glitch ефекта.

## Стъпки за настройка:

### 1. Добавете Digital Glitch към Post Process Profile

1. Отворете вашия **Post Process Profile** (например "VHS_Profile")
2. В Inspector, кликнете **Add effect...**
3. Изберете **Custom > Digital Glitch**
4. Ако не виждате "Digital Glitch" в менюто:
   - Рестартирайте Unity Editor
   - Проверете Console за грешки
   - Уверете се, че `DigitalGlitchPostProcess.cs` е компилиран без грешки

### 2. Настройте Digital Glitch параметрите

В Post Process Profile, намерете **Digital Glitch** и настройте:

- **Intensity** (0-1): Интензитет на ефекта
- **Speed**: Скорост на глич ефекта (по подразбиране: 10)
- **Horizontal Displacement**: Хоризонтално изместване (по подразбиране: 0.1)
- **Vertical Displacement**: Вертикално изместване (по подразбиране: 0.05)
- **Color Shift**: Интензитет на цветовото изместване (по подразбиране: 0.2)
- **Noise Intensity**: Интензитет на шума (по подразбиране: 0.3)
- **Block Size**: Размер на блоковете за глич (по подразбиране: 10)

**За тестване:** Включете ефекта и задайте **Intensity** на 1.0

### 3. Използване с VHSEffectController

`VHSEffectController` вече е актуализиран да работи с Digital Glitch:

1. Уверете се, че вашият **Post Process Volume** има **Post Process Profile** с добавения Digital Glitch ефект
2. `VHSEffectController` автоматично ще намери и контролира Digital Glitch ефекта
3. Използвайте `TriggerEffect()` за да активирате ефекта

### 4. Тестване

1. Влезте в Play Mode
2. Натиснете **T** (или зададения ключ) за да тествате ефекта
3. Ефектът трябва да се появява с глич ефекти

## Файлове:

- **DigitalGlitchPostProcess.cs** - C# скрипт за Post-Processing Stack v2
- **DigitalGlitchPostProcess.shader** - Shader за визуализация на ефекта
- **VHSEffectController.cs** - Актуализиран контролер (вече работи с Digital Glitch)

## Разлика от HDRP версията:

### Post-Processing Stack v2 (вашият проект):
- Използва `PostProcessVolume` компонент
- Използва `Post Process Profile` asset
- Ефектите се добавят чрез `PostProcessEffectSettings`
- Работи с Built-in Render Pipeline

### HDRP (не се използва в този проект):
- Използва `Volume` компонент (HDRP)
- Използва `Volume Profile` asset
- Ефектите се добавят чрез `VolumeComponent`
- Работи с HDRP Render Pipeline

## Ако не виждате Digital Glitch в менюто:

1. **Рестартирайте Unity Editor** (най-важно!)
2. Проверете Console за грешки:
   - Window > General > Console
   - Ако има червени грешки, поправете ги
3. Проверете дали скриптовете са компилирани:
   - `DigitalGlitchPostProcess.cs` - трябва да няма грешки
4. Проверете дали Post-Processing Stack v2 е инсталиран:
   - Window > Package Manager
   - Потърсете "Post Processing"
   - Трябва да е инсталиран

## Използване в код:

```csharp
// VHSEffectController автоматично работи с Digital Glitch
VHSEffectController controller = GetComponent<VHSEffectController>();

// Тригърване на ефекта
controller.TriggerEffect();

// Тригърване с персонализирана продължителност
controller.TriggerEffect(3.0f); // 3 секунди

// Спиране на ефекта
controller.StopEffect();
```

## Важно:

- Уверете се, че **Post-Processing Stack v2** е инсталиран в проекта
- Шейдърът трябва да се компилира без грешки
- След добавяне на ефекта, рестартирайте Unity за да се регистрира правилно

