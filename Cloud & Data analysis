<div dir="rtl">

# ארכיטקטורת שמירת נתונים ודשבורד – Sababich

## Overview

מערכת הדשבורד של Sababich היא כלי אנליטי למטפלים, המציג נתוני משחק מדידים בצורה ויזואלית וברורה.

המערכת מאפשרת:
- מעקב אחר ביצועי מטופלים לאורך זמן
- צפייה בהיסטוריית סשנים
- ניתוח ביצועים לפי רמות
- עבודה מלאה ללא שרת או חיבור אינטרנט

המערכת אינה מבצעת אבחון או פרשנות קלינית — אלא מציגה נתונים אובייקטיביים בלבד.

---

## Design Goals

- עבודה מלאה ללא שרת
- הפרדה בין `Gameplay` ל־`Analytics`
- תמיכה בעברית ואנגלית
- שמירת נתונים מקומית + גיבוי בענן
- פתיחה פשוטה וישירה מתוך המשחק

---

## Architecture

### Backend (`Unity / C#`)

אחראי על:
- איסוף נתונים בזמן המשחק
- יצירת סשנים
- שמירת נתונים
- ייצוא הדשבורד

```text
Assets/Scripts/Outputs_to_Cloud/
```

קבצים מרכזיים:
- `SessionDataCollector.cs`
- `SessionRecord.cs`
- `SessionExporter.cs`
- `CloudSessionHistory.cs`
- `ViewDashboardButton.cs`

### Frontend (`Dashboard`)

אחראי על:
- הצגת הנתונים
- גרפים
- פילטרים
- עיצוב

```text
Assets/StreamingAssets/Dashboard/
```

קבצים:
- `sababich_dashboard.html`
- `dashboard.css`
- `dashboard.js`

---

## זרימת נתונים מלאה

```text
שחקן נכנס למשחק (`Login`)
        │
        ├── משתמש רשום
        │       │
        │       ├── `Unity Authentication`
        │       │
        │       ├── טעינת נתונים מהענן
        │       │       ├── `resumeScene`
        │       │       └── מצב רמות
        │       │
        │       └── הפעלת `CloudProgressTracker`
        │
        └── אורח
                │
                └── `Anonymous Authentication`
                        └── ללא טעינת ענן

במהלך המשחק
        │
        ├── כל טעינת סצנה
        │       └── שמירת `resumeScene`
        │
        └── סיום רמה
                └── `SessionDataCollector.RecordLevelAttempt()`
                        └── שמירת נתוני רמה בזיכרון

סיום סשן
(`MainMenu` / `endScene`)
        │
        └── `FinalizeAndExport()`
                │
                ├── יצירת `SessionRecord`
                │
                ├── עדכון קובץ HTML מקומי
                │
                └── שמירה לענן (`Cloud Save`)
```

---

## איסוף נתונים בזמן המשחק

בכל סיום רמה מתבצעת קריאה ל־:

```csharp
SessionDataCollector.RecordLevelAttempt(...)
```

המערכת שומרת נתונים כגון:
- הצלחה / כישלון
- מטבעות
- זמן להגעה ליעד
- מנות שהוגשו
- מנות מושלמות
- לחיצות כפולות
- אירועי גלוטן
- לקוחות שהגיעו

הנתונים נשמרים זמנית בזיכרון בתוך:

```csharp
attempts
```

עד לסיום הסשן.

---

## יצירת Session

כאשר השחקן חוזר ל־`MainMenu` או מגיע ל־`endScene`:

```csharp
SessionDataCollector.FinalizeAndExport()
```

יוצר אובייקט מסוג:

```csharp
SessionRecord
```

המכיל:
- פרטי שחקן
- זמן ותאריך
- סצנה אחרונה
- כל הרמות שבוצעו באותו סשן

---

## שמירת נתונים

### קובץ מקומי (`Primary Storage`)

הדשבורד נשמר כקובץ HTML מקומי:

```text
Application.persistentDataPath/Dashboard/sababich_dashboard.html
```

הקובץ מתעדכן בסיום כל סשן.

### גיבוי לענן (`Unity Cloud Save`)

בנוסף לקובץ המקומי, הנתונים נשמרים גם בענן דרך:

```csharp
CloudSessionHistory.AppendSession()
```

השמירה בענן משמשת כגיבוי `Persistence` בלבד.

---

## יצירת הדשבורד (`HTML Export`)

הייצוא מתבצע דרך:

```csharp
SessionExporter.ExportSession()
```

### תהליך הייצוא

1. טעינת תבנית HTML מתוך:

```text
Assets/StreamingAssets/Dashboard/sababich_dashboard.html
```

2. טעינת סשנים קיימים מתוך קובץ הפלט

3. סינון סשנים לא תקינים / כפולים

4. הזרקת נתונים ל־HTML:

```js
const SABABICH_DATA = {
  sessions: [...],
  currentPlayer: "..."
};
```

5. כתיבת HTML חדש לדיסק

6. העתקת:
- `dashboard.css`
- `dashboard.js`

לתוך:

```text
persistentDataPath/Dashboard/
```

---

## פתיחת הדשבורד

לחיצה על כפתור הדשבורד ב־`MainMenu` מפעילה:

```csharp
ViewDashboardButton.OpenDashboard()
```

המערכת פותחת בדפדפן:

```text
file:///Application.persistentDataPath/Dashboard/sababich_dashboard.html
```

---

## Rendering (`dashboard.js`)

ה־JavaScript קורא את:

```js
SABABICH_DATA
```

ומבצע:
- יצירת כרטיסי סיכום
- בניית גרפים (`Chart.js`)
- יצירת פילטרים
- הצגת היסטוריית סשנים
- הצגת פירוט לכל רמה

---

## נתונים שנשמרים

### Raw Metrics

- שם מטופל
- תאריך סשן
- רמה שהושגה
- הצלחה / כישלון
- מטבעות
- זמן להגעה ליעד
- מנות שהוגשו
- מנות מושלמות
- לחיצות כפולות
- אירועי גלוטן
- לקוחות שהגיעו

### Derived Metrics

- מנות שגויות
- סך שגיאות
- זמן ממוצע להכנת מנה
- מדדי תגובה לחוקי גלוטן

---

## גרפים בדשבורד

המערכת מציגה:
- מטבעות לאורך זמן
- שגיאות לאורך זמן
- מנות מושלמות מול שגויות
- זמן להגעה ליעד
- אירועי גלוטן

הגרפים נוצרים באמצעות:

```text
Chart.js
```

---

## משתמש רשום מול אורח

| יכולת | משתמש רשום | אורח |
|---|---|---|
| שמירה לענן | ✅ | ❌ |
| קובץ HTML מקומי | ✅ | ✅ |
| `Resume` לסצנה | ✅ | ❌ |
| סנכרון מצב רמות | ✅ | ❌ |
| היסטוריית סשנים בענן | ✅ | ❌ |

---

## Development Approach

פיתוח המערכת בוצע בצורה איטרטיבית תוך שימוש ב־`Claude (AI)`.

התהליך כלל:
- כתיבת פרומפטים מדויקים
- `Root Cause Analysis`
- תיקונים מדורגים
- בדיקות בתוך Unity והדשבורד

נושאים מרכזיים שטופלו:
- טעינת `CSS` ו־`JavaScript`
- הזרקת נתונים ל־HTML
- מניעת סשנים ריקים
- תיקון זמן הייצוא
- סנכרון בין `Backend` ל־`Frontend`

---

## Current Status

המערכת תומכת כיום ב:
- הצגת נתוני משחק אמיתיים
- שמירת היסטוריית סשנים
- גרפים ופילטרים
- עבודה מלאה ללא שרת
- פתיחה ישירה מתוך המשחק

---

## Known Limitations

- היסטוריית הסשנים אינה נטענת אוטומטית מהענן למחשב חדש
- קובץ HTML אחד משותף לכל המשתמשים על אותו מחשב
- הערות מטפל נשמרות מקומית בלבד (`localStorage`)
- קיימת מגבלת `ring buffer` של עד 200 סשנים בענן

---

## Future Improvements

- סנכרון מלא בין ענן לדשבורד (תיתכן פגיעה בסודיות נתונים רפואיים)

</div>
