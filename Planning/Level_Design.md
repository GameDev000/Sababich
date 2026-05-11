<div dir="rtl">

# **Level Design**

להלן פירוט השלבים, אלמנטים משתנים והדרגתיות הקושי בהתקדמות בשלבים:

![Level Design Table](https://github.com/GameDev000/Sababich/blob/main/2025-11-18_231640.png)

כחלק מהמשחק ועלייה ברמות יתווסף אלמנט טיגון שידרוש מהשחקן לשמור על פוקוס תוך כדי הרכבת המנות: 

<table>
<thead>
  <tr>
    <th>רכיב</th>
    <th>אופן שימוש</th>
    <th>רמה שבה מופיע לראשונה</th>
    <th>הערות</th>
  </tr>
</thead>
<tbody>
  <tr>
    <td><strong>חציל</strong></td>
    <td>טיגון בינוני</td>
    <td>רמה 0 ומעלה</td>
    <td>דורש המתנה/הפיכה</td>
  </tr>
  <tr>
    <td><strong>צ'יפס תפו"א/ בטטה</strong></td>
    <td>טיגון קצר</td>
    <td>רמה 3</td>
    <td>נדרש להימנע מבלבול בין 2 הסוגים ושריפה של המטוגנים</td>
  </tr>
</tbody>
</table>

</div>

<div dir="rtl" lang="he">
  <h2>שלב 0 – Tutorial (ישראל)</h2>
  <p>
    דוכן בסיסי מאוד: לקוח יחיד, ביצה קשה וטחינה בלבד, ללא טיגון וללא הסחות דעת.
    המטבעות וזמן השלב מופיעים בשורת בתחתית המסך.
  </p>
</div>

<div dir="ltr">
<pre>
+---------------------------------------+
|                 C1                    |
|           (single customer)           |
|---------------------------------------|
|             SERVE COUNTER             |
|---------------------------------------|
|   PITA   EGG   EGGPL AMBA  TAHINI     |
|                 SALAD                 |
|               PREP AREA               |
+---------------------------------------+
| TIME:  ∞ (tutorial)    COINS: 000     |
+---------------------------------------+
</pre>
</div>

<div dir="rtl" lang="he">
  <p><strong>אגדה:</strong></p>
  <ul>
    <li>לקוח יחיד בתור - c1</li>
    <li>SERVE COUNTER – דלפק הגשה והרכבת סביח</li>
    <li>PITA – פיתות</li>
    <li>AGGPL – חציל</li>
    <li>AMBA – עמבה</li>
    <li>SALAD – סלט</li>
    <li>EGG (hard) – ביצה קשה</li>
    <li>TAHINI – טחינה</li>
    <li>PREP AREA – אזור עבודה כללי</li>
    <li>שורת  תחתונה:
      <ul>
        <li><strong>COINS</strong> – סך המטבעות שנצברו</li>
      </ul>
    </li>
  </ul>
  <hr>
</div>

<div dir="rtl" lang="he">
  <h2>שלב 1 – קל (ישראל)</h2>
  <p>
    עומס קל: ביצה קשה, חציל וטחינה.  
    כאן בפעם הראשונה מופיע אלמנט הטיגון (חציל בלבד).  
    אין עדיין הסחות דעת כבדות, רק רעש רקע כללי.
  </p>
</div>

<div dir="ltr">
<pre>
+---------------------------------------+
|                    C1                 |
|                  (cust)               |
|---------------------------------------|
|             SERVE COUNTER             |
|---------------------------------------|
|   PITA   EGG   EGGPL  AMBA  TAHINI    |
|                SALAD                  |
|               FRY ZONE                |
|          [FRY-EGGPL ONLY]             |
+---------------------------------------+
| TIME: 01:30 avg       COINS: 000      |
+---------------------------------------+
</pre>
</div>

<div dir="rtl" lang="he">
  <p><strong>אגדה:</strong></p>
  <ul>
    <li>עשלושה לקוחות בתור בנפרד - c1-c3</li>
    <li>SERVE COUNTER – דלפק שבו מרכיבים את המנה ללקוח.</li>
    <li>PITA – פיתות.</li>
    <li>EGG – ביצים קשות מוכנות.</li>
    <li>AGGPL – חציל</li>
    <li>AMBA – עמבה</li>
    <li>SALAD – סלט</li>
    <li>TAHINI – טחינה.</li>
    <li>שורת UI תחתונה:
      <ul>
        <li><strong>COINS</strong> – המטבעות שהשחקן צובר.</li>
      </ul>
    </li>
  </ul>
  <hr>
</div>

<div dir="rtl" lang="he">
  <h2>שלב 2 – בינוני (יפן)</h2>
  <p>
    יותר לקוחות רק 2 במקביל, מצטרף רכיב חדש – סויה.  
    טיגון חציל נמשך, ולראשונה נכנסות הסחות דעת ברורות: טלפון, מוזיקה, הודעות רקע – באזור צד ייעודי.
  </p>
</div>

<div dir="ltr">
<pre>
+-----------------------------------------------+
|               C1              C2             |
|-----------------------------------------NOISE|
|             SERVE COUNTER               AREA  |
|-----------------------------------------TEL   |
| PITA    EGG    EGGPL    TAHINI  SOY     ♫    |
|                 SALAD  AMBA                   |
|           FRY ZONE (EGGPL ONLY)             |
+---------------------------------------------+
| TIME: 02:00 avg            COINS: 000       |
+---------------------------------------------+
</pre>
</div>

<div dir="rtl" lang="he">
  <p><strong>אגדה:</strong></p>
  <ul>
    <li>עד חמישה לקוחות בתור כאשר לכל היותר 2 במקביל - c1-c5</li>
    <li>SERVE COUNTER – דלפק הגשה.</li>
    <li>PITA – פיתות.</li>
    <li>EGG – ביצים קשות.</li>
    <li>AGGPL – חציל</li>
    <li>AMBA – עמבה</li>
    <li>SALAD – סלט</li>
    <li>TAHINI – טחינה.</li>
    <li>SOY – סויה.</li>
    <li>NOISE AREA / TEL / ♫ – אזור הסחות הדעת:
      <ul>
        <li>צלצול טלפון</li>
        <li>מוזיקת רקע</li>
        <li>קריאות רקע</li>
      </ul>
    </li>
    <li>שורת תחתונה:
      <ul>
        <li><strong>COINS</strong> – סכום המטבעות בשלב</li>
      </ul>
    </li>
  </ul>
  <hr>
</div>

<div dir="rtl" lang="he">
  <h2>שלב 3 – קשה (ארה"ב)</h2>
  <p>
    עומס גבוה: תור ארוך, כל הרכיבים במשחק פעילים (ביצה קשה, חציל, טחינה, סויה, צ'יפס תפו"א/בטטה).  
    הטיגון מורכב יותר – חציל וצ'יפס במקביל, אזור הסחות גדול ורועש, והמטבעות קריטיים לעבור את השלב.
  </p>
</div>
<div dir="rtl" lang="he">
<pre>
+---------------------------------------------------+
|           C1             C2            C3         |
|------------------------------------------------NOI|
|                 SERVE COUNTER                 SE  | 
|------------------------------------------------ CT|
| PITA    EGG    EGGPL    TAHINI  AMBA   FRIES      |
|                       SALAD                       |
|    FRY ZONE A           |      FRY ZONE B         |
|    [EGGPL]              |   [FRIES POT/BAT]       |
+-------------------------------------------------+
| TIME: 02:30 avg      COINS: 000                   |
+-------------------------------------------------+
</pre>
</div>

<div dir="rtl" lang="he">
  <p><strong>אגדה:</strong></p>
  <ul>
    <li>עד שמונה לקוחות בתור , כאשר לכל היותר 3 במקביל - c1-c8</li>
    <li>SERVE COUNTER – דלפק הכנה.</li>
    <li>PITA – פיתות.</li>
    <li>EGG – ביצה קשה.</li>
    <li>EGGPL – חציל.</li>
    <li>TAHINI – טחינה.</li>
    <li>FRIES – צ'יפס.</li>
    <li>AMBA – עמבה</li>
    <li>SALAD – סלט</li>
    <li>FRY ZONE A – טיגון לחציל.</li>
    <li>FRY ZONE B – טיגון צ'יפס.</li>
    <li>NOISE AREA – אזור הסחות (צעקות, מוזיקה חזקה).</li>
    <li>שורת תחתונה:
      <div dir="rtl" lang="he">
      <ul>
        <li><strong>COINS</strong> – המטבעות שהשחקן צובר.</li>
      </ul>
    </li>
  </ul>
</div>
