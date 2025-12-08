# Technical Documentation - Sababich Game

Complete documentation of all classes and functions in the game, organized by folders with links to relevant code lines.

---

## Folder Structure

```
Assets/Scripts/
├── Enemies/      # Customer logic and orders
├── Manager/      # Core game managers
├── Player/       # Player actions and frying
└── Util/         # Utilities and UI
```

---

## Enemies Folder

### [MoveToCounter.cs](Assets/Scripts/Enemies/MoveToCounter.cs)

**Description:** Handles customer movement toward the counter in a linear way.
Uses Vector2.MoveTowards to move the customer gradually toward the target.

**Functions:**
- [`Update()`](Assets/Scripts/Enemies/MoveToCounter.cs#L8-L15) - **Key function:** Updates customer position every frame by calculating the distance to target and moving the object at constant speed times elapsed time (`speed * Time.deltaTime`). If target is not set, exits early without updating position to prevent errors.

---

### [CheckOrder.cs](Assets/Scripts/Enemies/CheckOrder.cs)

**Description:** Validates the customer's order against selected ingredients.
Manages the correct ingredient list and compares it to the player's current selection.

**Functions:**
- [`OnMouseDown()`](Assets/Scripts/Enemies/CheckOrder.cs#L13-L41) - **Key function:** Triggered when clicking on customer, checks if selected ingredient list matches the correct order using `IsSelectionMatching()`. If order is correct, clears the ingredient list and notifies CustomerMoodTimer that customer was served successfully, and updates TutorialManager. If wrong, prints error message with entered order and correct order for debugging.

---

### [CustomerMoodTimer.cs](Assets/Scripts/Enemies/CustomerMoodTimer.cs)

**Description:** Manages customer mood changes over time and their exit process.
Customer goes through different mood states at set intervals until they leave or get served.

**Functions:**
- [`Update()`](Assets/Scripts/Enemies/CustomerMoodTimer.cs#L35-L56) - **Key function:** Updates mood timer every frame and changes sprite according to set time intervals (`interval`). When customer reaches final mood state (most angry - `currentFace == moodFaces.Length - 1`), triggers walking and exit coroutine. Uses `moodStopped` flag to stop updates after customer starts walking.
- [`CustomerServed()`](Assets/Scripts/Enemies/CustomerMoodTimer.cs#L59-L64) - **Key function:** Called when customer gets correct service, stops mood changes by setting `moodStopped = true`, resets `currentFace` to 0, and triggers customer's happy exit process through `WalkAwayAndNotify()` coroutine.
- [`WalkAwayAndNotify()`](Assets/Scripts/Enemies/CustomerMoodTimer.cs#L66-L81) - **Key coroutine:** Performs customer walking animation to the right for `duration` seconds. Uses `Vector3.Lerp` for smooth movement from current position 10 units to the right, updates position each frame until movement completes, then triggers `OnReachedExit()` to notify customer left the scene.
- [`ResetCustomer()`](Assets/Scripts/Enemies/CustomerMoodTimer.cs#L91-L103) - **Key function:** Resets all customer state variables (timers, mood, position) back to initial state so same customer object can be reused without creating new instance. This enables efficient object reuse (object pooling pattern).

---

## Manager Folder

### [ClickManager.cs](Assets/Scripts/Manager/clickMananger.cs)

**Description:** Manages all clicks in game by converting mouse positions to world coordinates.
Detects objects with Item component and triggers their corresponding function.

**Functions:**
- [`Update()`](Assets/Scripts/Manager/clickMananger.cs#L5-L24) - **Key function:** Checks every frame if there's a left mouse click (`Input.GetMouseButtonDown(0)`), converts mouse position from screen coordinates to 2D world coordinates using `Camera.main.ScreenToWorldPoint`, performs Raycast2D to find clicked object, and if object with `Item` component is found calls its `OnClick()` function. This is the main entry point for all mouse interactions in the game.

---

### [GameFlowManager.cs](Assets/Scripts/Manager/GameFlowManager.cs)

**Description:** Manages different game phases and updates on-screen instructions accordingly.
Holds current state (`CurrentPhase`) and coordinates between different gameplay stages.

**Functions:**
- [`SetPhase(GamePhase newPhase)`](Assets/Scripts/Manager/GameFlowManager.cs#L14-L50) - **Key function:** Updates current game phase and shows appropriate UI instructions based on phase using switch statement. Each phase (adding eggplants, frying, assembling dish, serving, next customer) displays different guidance text to player through `UIInstructions.SetInstructions()`. This helps player understand what next step to perform in game.

---

### [MainMenu.cs](Assets/Scripts/Manager/MainMenu.cs)

**Description:** Manages game's main menu and scene transitions.
Provides callback functions for main menu buttons.

**Functions:**
- [`OnTutorialButtonClicked()`](Assets/Scripts/Manager/MainMenu.cs#L6-L8) - Loads tutorial scene (`TutorialScene`) when clicking tutorial button using `SceneManager.LoadScene`

---

### [MusicPlayer.cs](Assets/Scripts/Manager/MusicPlayer.cs)

**Description:** Singleton that manages game music and keeps it between scene changes.
Uses DontDestroyOnLoad to ensure music continuity throughout entire game.

**Functions:**
- [`Awake()`](Assets/Scripts/Manager/MusicPlayer.cs#L8-L17) - **Key function:** Implements Singleton pattern by checking if MusicPlayer instance already exists, and if so destroys new copy (`Destroy(this.gameObject)`). Otherwise, sets itself as Instance, initializes AudioSource reference, and sets object to survive between scene changes using `DontDestroyOnLoad`. This ensures music continues smoothly without interruption.

---

### [ScoreManager.cs](Assets/Scripts/Manager/ScoreManager.cs)

**Description:** Singleton that manages player score (money/coins) and updates score display.
Provides central interface for adding money and updating UI in real-time.

**Functions:**
- [`AddMoney(int amount)`](Assets/Scripts/Manager/ScoreManager.cs#L24-L27) - Adds money amount to current score and calls display update

---

### [SelectionList.cs](Assets/Scripts/Manager/SelectionList.cs)

**Description:** Singleton that manages ingredient list player selected for assembling sababich.
Keeps selection order and displays it on screen, including validation that first ingredient must be pita.

**Functions:**
- [`TryAddIngredient(string ingredientName)`](Assets/Scripts/Manager/SelectionList.cs#L28-L42) - **Key function:** Tries to add ingredient to list, first converts name to lowercase, checks if list is empty and ingredient is not pita (prevents starting assembly without pita), adds ingredient and updates display. Returns `true` if addition succeeded and `false` if failed. This ensures every sandwich starts with pita as base.
- [`IsSelectionMatching(List<string> correctOrder)`](Assets/Scripts/Manager/SelectionList.cs#L63-L81) - **Key function:** Compares selected ingredient list to correct order list. First checks that list lengths match (if not returns `false`), then iterates through each ingredient in correct order and checks it exists in selected list (`Contains`) - note that check is order-independent! Returns `true` only if all ingredients from correct order are found in list.

---

### [TutorialManager.cs](Assets/Scripts/Manager/TutorialManager.cs)

**תיאור:** Singleton המנהל את כל תהליך ההדרכה במשחק בשלבים מוגדרים.
מתאם בין טיגון חצילים, הרכבת סנדוויץ', ושירות לקוחות תוך מתן חיצים מנחים לשחקן.

**פונקציות:**
- [`OnIngredientClicked(Item item, string ingredientName)`](Assets/Scripts/Manager/TutorialManager.cs#L65-L86) - **פונקציה מרכזית:** נקודת כניסה מרכזית לכל לחיצה על מרכיב במשחק. מנתבת את הלחיצה לטיפול המתאים בהתאם לשלב ההדרכה הנוכחי (FryEggplant/BuildSandwich/Done) דרך switch statement. זה מאפשר לוגיקה שונה ומותאמת לכל שלב במדריך - בשלב הטיגון רק חצילים רלוונטיים, בשלב בניית הסנדוויץ' יש סדר מוגדר וכו'.
- [`OnEggplantTrayFull()`](Assets/Scripts/Manager/TutorialManager.cs#L117-L135) - **פונקציה מרכזית:** נקראת כאשר מגש החצילים מתמלא אחרי לקיחת החציל מהמחבת. מנטרלת את האפשרות ללחוץ על החציל (`SetClickable(false)`), מעבירה את המדריך לשלב בניית הסנדוויץ' (`phase = TutorialPhase.BuildSandwich`), מפעילה חיץ על המרכיב הראשון שצריך להוסיף (`otherItems[0]`), ומציגה את הלקוח על המסך. זהו נקודת מעבר קריטית בין שני שלבי המדריך.
- [`HandleBuildSandwichClick(Item item, string ingredientName)`](Assets/Scripts/Manager/TutorialManager.cs#L137-L188) - **פונקציה מרכזית:** מטפל בלחיצות בשלב הרכבת הסנדוויץ', משווה את המרכיב שנלחץ למרכיב הצפוי (`buildOrderNames[buildStep]`). אם המרכיב שגוי מדפיסה הודעת שגיאה וחוזרת. אם נכון, מכבה את החץ הנוכחי, מקדם את הצעד הבא (`buildStep++`), ומציג חץ למרכיב הבא. כאשר כל המרכיבים הוסיפו בסדר הנכון, מפעיל חץ על הלקוח להגשת המנה ומעדכן את GameFlowManager לשלב הגשה.
- [`CustomerOrderServed()`](Assets/Scripts/Manager/TutorialManager.cs#L191-L199) - **פונקציה מרכזית:** נקראת כאשר מגישים את המנה ללקוח בהצלחה. מוסיפה 10 מטבעות לניקוד השחקן דרך ScoreManager, מגדילה את מונה הלקוחות המשורתים (`servedCustomers++`), ומכינה את המערכת להמשך המדריך או לסיומו בהתאם למספר הלקוחות שהוגדר.

---

### [GamePhase.cs](Assets/Scripts/Manager/GamePhase.cs)

**תיאור:** Enum המגדיר את השלבים השונים במחזור המשחק.
משמש ל-GameFlowManager לניהול המעברים בין שלבים ועדכון ההוראות.

**ערכים:**
- `AddRowEggplant` - שלב הוספת חצילים גולמיים לאזור הטיגון
- `FryingEggplant` - שלב טיגון החצילים והמתנה לסיום
- `AssembleDish` - שלב הרכבת המנה עם כל המרכיבים
- `ServeCustomer` - שלב הגשת המנה המוכנה ללקוח
- `NextCustomer` - שלב מעבר ללקוח הבא והכנה לסיבוב חדש

---

## תיקיית Player

### [ItemScript.cs](Assets/Scripts/Player/ItemScript.cs)

**תיאור:** מחלקה המייצגת מרכיב/פריט במשחק שניתן ללחוץ עליו (חציל, ביצה, סלט וכו').
מטפלת בלוגיקת הלחיצה, הוספה לרשימת הבחירה והודעה ל-TutorialManager לצורכי מעקב.

**פונקציות:**
- [`OnClick()`](Assets/Scripts/Player/ItemScript.cs#L8-L43) - **פונקציה מרכזית:** מטפלת בלחיצה על מרכיב, בודקת אם המרכיב ניתן ללחיצה (`isClickable`), מוסיפה אותו לרשימת הבחירה דרך `SelectionList.Instance.TryAddIngredient()` (למעט פריטים מיוחדים כמו `eggplantrow` ו-`fryzone` שאינם מרכיבי אוכל), ומודיעה ל-TutorialManager על הפעולה באמצעות `OnIngredientClicked()`. מציגה הודעות debug מפורטות בכל שלב לצורכי דיבאג ומעקב אחר התהליך.

---

### [FryZoneEggplant.cs](Assets/Scripts/Player/FryZoneEggplant.cs)

**תיאור:** מחלקה המנהלת את אזור הטיגון של החצילים ואת מצבי הטיגון השונים (Empty/Frying/Ready).
מטפלת במעבר בין מצבים, שינוי ספרייטים, ניהול טיימר הטיגון, ואינטראקציה עם מגש החצילים.

**פונקציות:**
- [`StartFry()`](Assets/Scripts/Player/FryZoneEggplant.cs#L46-L55) - **פונקציה מרכזית:** מתחילה את תהליך הטיגון. בודקת שהמצב הנוכחי ריק (`state == FryState.Empty`) לפני תחילת טיגון חדש למניעת באגים, מאפסת את הטיימר ל-0, ומעדכנת את המצב ל-Frying דרך `SetState()` מה שגורם להצגת ספרייט חציל גולמי. זהו נקודת ההתחלה של תהליך הטיגון כולו.
- [`SetState(FryState newState)`](Assets/Scripts/Player/FryZoneEggplant.cs#L63-L92) - **פונקציה מרכזית:** מעדכנת את מצב הטיגון הפנימי, משנה את הספרייט המתאים בהתאם למצב החדש - מסתיר ספרייט במצב Empty, מציג חציל גולמי ב-Frying, מציג חציל מטוגן ב-Ready. במצב Ready גם מודיעה ל-TutorialManager שהחציל מוכן דרך `OnEggplantFried()`. זוהי נקודת הבקרה המרכזית לכל המעברים בין מצבי הטיגון.
- [`OnMouseDown()`](Assets/Scripts/Player/FryZoneEggplant.cs#L94-L107) - **פונקציה מרכזית:** נקראת כאשר לוחצים על המחבת. פועלת רק כאשר החציל מוכן (`state == FryState.Ready`), מנקה את המחבת חזרה למצב ריק, וממלאת את מגש החצילים דרך `eggplantTray.FillFromPan()`. זהו השלב האחרון בתהליך הטיגון שבו השחקן לוקח את החצילים המוכנים.

---

### [EggplantTray.cs](Assets/Scripts/Player/EggplantTray.cs)

**תיאור:** מחלקה המייצגת את מגש החצילים המטוגנים המוכן לשימוש.
מנהלת את המעבר בין מצב ריק למצב מלא אחרי לקיחת חצילים מהמחבת.

**פונקציות:**
- [`FillFromPan()`](Assets/Scripts/Player/EggplantTray.cs#L27-L43) - **פונקציה מרכזית:** ממלאת את המגש מהמחבת. משנה את הספרייט למגש מלא (`fullTraySprite`), מפעילה את הרינדור (`trayRenderer.enabled = true`), ומודיעה ל-TutorialManager שהמגש מלא דרך `OnEggplantTrayFull()` מה שמתחיל את השלב הבא במדריך. מציגה הודעות debug מפורטות לעקיבה אחר התהליך. זוהי נקודת המעבר מטיגון להרכבת סנדוויץ'.

---

## תיקיית Util

### [UIInstructions.cs](Assets/Scripts/Util/UIInstructions.cs)

**תיאור:** מחלקה פשוטה המנהלת את תצוגת טקסט ההוראות על המסך.
מעדכנת את ה-TextMeshPro בהתאם לשלב המשחק הנוכחי.

**פונקציות:**
- [`SetInstructions(string text)`](Assets/Scripts/Util/UIInstructions.cs#L12-L20) - מעדכנת את טקסט ההוראות המוצג על המסך. תחילה מנקה את ה-mesh הקודם (`ClearMesh()`) ואז מגדירה את הטקסט החדש

---

### [FryTimerUI.cs](Assets/Scripts/Util/FryTimerUI.cs)

**תיאור:** מחלקה המציגה את טיימר הטיגון ב-UI בצורה ויזואלית אינטואיטיבית.
משתמשת ב-Image.fillAmount כדי להציג התקדמות הטיגון כמעגל שמתרוקן.

**פונקציות:**
- [`Start()`](Assets/Scripts/Util/FryTimerUI.cs#L9-L21) - **פונקציה מרכזית:** מאתחלת את תצוגת הטיימר. מחפשת את קומפוננטת ה-Image באובייקט אם לא הוגדרה ידנית, ומגדירה אותה כמוסתרת (`enabled = false`) עם מילוי מלא (`fillAmount = 1f`) בהתחלה. זה מבטיח שהטיימר לא יופיע כשאין טיגון פעיל.
- [`Update()`](Assets/Scripts/Util/FryTimerUI.cs#L23-L41) - **פונקציה מרכזית:** מעדכנת את תצוגת הטיימר בכל פריים. בודקת אם יש טיגון פעיל (`fryZone.IsFrying`), אם לא - מסתירה את הטיימר. אם כן - מציגה אותו ומעדכנת את ה-fillAmount בהתאם להתקדמות הטיגון (`1f - fryZone.FryProgress`) כך שהמעגל מתרוקן בהדרגה עד 0 כשהטיגון מסתיים. זה נותן פידבק ויזואלי ברור לשחקן.

---

### [MuteButton.cs](Assets/Scripts/Util/MuteButton.cs)

**תיאור:** מחלקה המנהלת את כפתור השתקת/הפעלת המוזיקה בממשק.
מעדכנת את האייקון של הכפתור בהתאם למצב הנוכחי של המוזיקה.

**פונקציות:**
- [`UpdateIcon()`](Assets/Scripts/Util/MuteButton.cs#L22-L36) - **פונקציה מרכזית:** מעדכנת את ספרייט הכפתור בהתאם למצב ההשתקה הנוכחי של המוזיקה. בודקת את הערך של `audioSource.mute` ובוחרת בין ספרייט mute (מושתק) ל-unmute (מפעיל). זה נותן פידבק ויזואלי ברור לשחקן על מצב המוזיקה.

---

## תרשים זרימת המשחק

```
1. תפריט ראשי (MainMenu)
   │
   ├─→ לחיצה על "מדריך" → TutorialScene
   │
   └─→ לחיצה על "משחק" → GameScene (עתידי)

2. מדריך (TutorialManager)
   │
   ├─→ שלב 1: טיגון חצילים
   │   ├─ לחיצה על חציל → הוספה למחבת
   │   ├─ טיגון אוטומטי (5 שניות)
   │   └─ לחיצה על מחבת → העברה למגש
   │
   ├─→ שלב 2: הרכבת סנדוויץ'
   │   ├─ פיתה (חובה ראשון)
   │   ├─ ביצה
   │   ├─ סלט
   │   ├─ חציל מטוגן
   │   ├─ עמבה
   │   └─ טחינה
   │
   ├─→ שלב 3: הגשה ללקוח
   │   ├─ לחיצה על לקוח
   │   ├─ בדיקת הזמנה (CheckOrder)
   │   └─ קבלת ניקוד (+10 מטבעות)
   │
   └─→ חזרה על התהליך (3 לקוחות)
       │
       └─ חזרה לתפריט ראשי
```

---

## קישורים מהירים לפונקציות מרכזיות

### מערכת לקוחות
- [תנועת לקוח לדלפק](Assets/Scripts/Enemies/MoveToCounter.cs#L8-L15)
- [בדיקת הזמנה נכונה](Assets/Scripts/Enemies/CheckOrder.cs#L13-L41)
- [שינוי מצבי רוח](Assets/Scripts/Enemies/CustomerMoodTimer.cs#L35-L56)
- [יציאת לקוח מהסצנה](Assets/Scripts/Enemies/CustomerMoodTimer.cs#L66-L81)

### מנהלי משחק
- [מערכת לחיצות מרכזית](Assets/Scripts/Manager/clickMananger.cs#L5-L24)
- [ניהול שלבי משחק](Assets/Scripts/Manager/GameFlowManager.cs#L14-L50)
- [השמעת מוזיקה רציפה](Assets/Scripts/Manager/MusicPlayer.cs#L8-L17)
- [הוספת מרכיב לסנדוויץ'](Assets/Scripts/Manager/SelectionList.cs#L28-L42)
- [השוואת הזמנה](Assets/Scripts/Manager/SelectionList.cs#L63-L81)

### מערכת הדרכה
- [ניתוב לחיצות במדריך](Assets/Scripts/Manager/TutorialManager.cs#L65-L86)
- [מעבר לשלב הרכבה](Assets/Scripts/Manager/TutorialManager.cs#L117-L135)
- [הרכבה עם חיצים](Assets/Scripts/Manager/TutorialManager.cs#L137-L188)
- [סיום הגשה מוצלחת](Assets/Scripts/Manager/TutorialManager.cs#L191-L199)

### מערכת טיגון
- [התחלת טיגון](Assets/Scripts/Player/FryZoneEggplant.cs#L46-L55)
- [מעבר בין מצבי טיגון](Assets/Scripts/Player/FryZoneEggplant.cs#L63-L92)
- [לקיחת חצילים מטוגנים](Assets/Scripts/Player/FryZoneEggplant.cs#L94-L107)
- [מילוי מגש החצילים](Assets/Scripts/Player/EggplantTray.cs#L27-L43)

### UI ופידבק
- [טיימר טיגון ויזואלי](Assets/Scripts/Util/FryTimerUI.cs#L23-L41)
- [עדכון הוראות](Assets/Scripts/Util/UIInstructions.cs#L12-L20)

---

## הערות טכניות

### דפוסי תכנון (Design Patterns)
<div dir="rtl" lang="he">

- **Singleton Pattern:** MusicPlayer, ScoreManager, SelectionList, TutorialManager
  - מבטיח instance יחיד לכל מנהל
  - גישה גלובלית דרך Instance
  
- **State Machine:** FryZoneEggplant (Empty → Frying → Ready)
  - מעברים מוגדרים בין מצבים
  - התנהגות שונה לכל מצב

</div>

<div dir="rtl" lang="he">

- **Observer Pattern:** TutorialManager מקבל התראות מאובייקטים שונים
  - OnIngredientClicked, OnEggplantFried, CustomerOrderServed
  - ריכוז לוגיקה בנקודה אחת

</div>

### אדריכלות
<div dir="rtl" lang="he">

- **ניהול מצב מרכזי:** כל ה-Managers מחזיקים מצב גלובלי
- **אירועים:** שימוש בקריאות callback לתקשורת בין מערכות
- **הפרדת אחריות:** כל מחלקה אחראית על תחום מוגדר
- **שימוש חוזר:** Object pooling למשל ב-ResetCustomer()

</div>

### טכנולוגיות Unity
<div dir="rtl" lang="he">

- **Physics2D:** Raycast לזיהוי לחיצות
- **Coroutines:** אנימציות חלקות (WalkAwayAndNotify)
- **SceneManagement:** מעבר בין סצנות
- **TextMeshPro:** טקסט באיכות גבוהה
- **Sprites:** ניהול גרפיקה 2D

</div>

---

## המשך פיתוח

### תכונות מתוכננות
- משחק ראשי עם רמות קושי
- מרכיבים נוספים
- הסחות דעת (טלפון, רעש)
- מערכת ניקוד מתקדמת
- טבלת שיאים

### אזורים לשיפור
- ניהול שגיאות מתקדם
- מערכת אירועים מרכזית (EventManager)
- פיצול TutorialManager למחלקות קטנות יותר
- הוספת בדיקות יחידה (Unit Tests)
- אופטימיזציה לביצועים

---

**עדכון אחרון:** דצמבר 2025  
**גרסה:** 1.0  
<div dir="rtl" lang="he">

**מנוע:** Unity 2022+

</div>

</div>
