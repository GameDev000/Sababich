// ── Language strings ──────────────────────────────────────────────────────────
const LABELS = {
  he: {
    title: 'הישגי מטופל - סבביח',
    langBtn: 'English',
    patientLabel: 'מטופל:',
    patientAll: 'כל המטופלים',
    filtersToggle: '▼ סינון',
    filterDateFrom: 'מתאריך',
    filterDateTo: 'עד תאריך',
    filterLevel: 'רמה',
    filterResult: 'תוצאה',
    filterAll: 'הכל',
    filterPass: 'עבר',
    filterFail: 'נכשל',
    resetFilters: 'אפס סינון',
    cardSessions: 'סשנים',
    cardPassed: 'רמה אחרונה שעבר',
    cardAvgCoins: 'ממוצע מטבעות',
    cardAvgTime: 'זמן ממוצע להכנת מנה',
    cardErrors: 'שגיאות סה״כ',
    cardGluten: 'טעויות גלוטן',
    cardLastLevel: 'רמה אחרונה שהושגה',
    cardTotalServed: 'מנות שהוגשו סה״כ',
    sortLabel: 'מיון:',
    sortRecent: 'אחרון ראשון',
    sortOldest: 'ישן ראשון',
    sortCoinsHigh: 'מטבעות ↓',
    sortCoinsLow: 'מטבעות ↑',
    sortTime: 'זמן למטרה',
    sortDuplicates: 'קליקים כפולים',
    sortGluten: 'טעויות גלוטן',
    sortPerfect: 'אחוז מנות מושלמות',
    sessionTitle: 'היסטוריית סשנים',
    detailTitle: 'פרטי סשן',
    metaDate: 'תאריך:',
    metaScene: 'סצנה אחרונה:',
    metaGuest: 'אורח',
    tableLevel: 'רמה',
    tableResult: 'תוצאה',
    tableCoins: 'מטבעות',
    tableTime: 'זמן (שנ׳)',
    tableTotal: 'הוגש סה״כ',
    tablePerfect: 'מושלם',
    tableIncorrect: 'שגוי',
    tableDuplicate: 'קליקים כפולים',
    tableGlutenApp: 'ילד גלוטן הופיע',
    tableGlutenServed: 'הוגש גלוטן',
    tableGlutenHandled: 'לא הוגש גלוטן',
    tableAvgPrep: 'זמן הכנה ממוצע',
    obsDishes: 'מנות שהוגשו',
    obsClicks: 'קשב וחזרתיות',
    obsGluten: 'הימנעות מהגשה לילד רגיש לגלוטן',
    obsOutcome: 'תוצאת רמה',
    obsPlanning: 'תכנון ויעילות משימה',
    chartsTitle: 'גרפים',
    chartCoins: 'מטבעות לאורך זמן',
    chartErrors: 'שגיאות לאורך זמן',
    chartPerfect: 'מנות מושלמות מול שגויות',
    chartTime: 'זמן למטרה לאורך זמן',
    chartGluten: 'אירועי גלוטן לאורך זמן',
    notesTitle: 'הערות מטפל',
    notesPlaceholder: 'הוסף הערות לסשן זה...',
    notesSaved: 'נשמר אוטומטית',
    printBtn: 'הדפסה',
    pass: 'עבר',
    fail: 'נכשל',
    notAttempted: 'לא שוחק',
    na: 'לא זמין',
  },
  en: {
    title: 'Patient Progress - Sababich',
    langBtn: 'עברית',
    patientLabel: 'Patient:',
    patientAll: 'All patients',
    filtersToggle: '▼ Filters',
    filterDateFrom: 'From date',
    filterDateTo: 'To date',
    filterLevel: 'Level',
    filterResult: 'Result',
    filterAll: 'All',
    filterPass: 'Passed',
    filterFail: 'Failed',
    resetFilters: 'Reset',
    cardSessions: 'Sessions',
    cardPassed: 'Last Level Passed',
    cardAvgCoins: 'Avg Coins',
    cardAvgTime: 'Avg Dish Prep Time',
    cardErrors: 'Total Errors',
    cardGluten: 'Gluten Mistakes',
    cardLastLevel: 'Last Level Reached',
    cardTotalServed: 'Total Dishes Served',
    sortLabel: 'Sort:',
    sortRecent: 'Most recent',
    sortOldest: 'Oldest first',
    sortCoinsHigh: 'Coins ↓',
    sortCoinsLow: 'Coins ↑',
    sortTime: 'Time to target',
    sortDuplicates: 'Duplicate clicks',
    sortGluten: 'Gluten mistakes',
    sortPerfect: 'Perfect order rate',
    sessionTitle: 'Session History',
    detailTitle: 'Session Detail',
    metaDate: 'Date:',
    metaScene: 'Last scene:',
    metaGuest: 'Guest',
    tableLevel: 'Level',
    tableResult: 'Result',
    tableCoins: 'Coins',
    tableTime: 'Time (s)',
    tableTotal: 'Total Served',
    tablePerfect: 'Perfect',
    tableIncorrect: 'Incorrect',
    tableDuplicate: 'Dup. Clicks',
    tableGlutenApp: 'Gluten Appeared',
    tableGlutenServed: 'Gluten Served',
    tableGlutenHandled: 'Gluten Not Served',
    tableAvgPrep: 'Avg Prep Time',
    obsDishes: 'Dishes Served',
    obsClicks: 'Attention & Repetition',
    obsGluten: 'Avoiding Serving Gluten-Sensitive Child',
    obsOutcome: 'Level Outcome',
    obsPlanning: 'Planning / Task Efficiency',
    chartsTitle: 'Charts',
    chartCoins: 'Coins over time',
    chartErrors: 'Errors over time',
    chartPerfect: 'Perfect vs Incorrect Dishes',
    chartTime: 'Time to Target over Sessions',
    chartGluten: 'Gluten Events over Sessions',
    notesTitle: 'Therapist Notes',
    notesPlaceholder: 'Add notes for this session...',
    notesSaved: 'Auto-saved',
    printBtn: 'Print',
    pass: 'Pass',
    fail: 'Fail',
    notAttempted: 'Not played',
    na: 'N/A',
  }
};

// ── State ─────────────────────────────────────────────────────────────────────
let lang = 'he';
let selectedPatient = '';
let selectedSession = null;
let sortMode = 'recent';
let filterDateFrom = '';
let filterDateTo = '';
let filterLevel = 0;
let filterResult = 'all';
let coinsChart   = null;
let errorsChart  = null;
let perfectChart = null;
let timeChart    = null;
let glutenChart  = null;

// ── Helpers ───────────────────────────────────────────────────────────────────
const L = () => LABELS[lang];

function fmt(val) {
  if (val === null || val === undefined || val === -1) return L().na;
  return String(val);
}

function fmtDate(iso) {
  if (!iso) return '';
  const d = new Date(iso);
  return d.toLocaleDateString(lang === 'he' ? 'he-IL' : 'en-GB') +
         ' ' + d.toLocaleTimeString(lang === 'he' ? 'he-IL' : 'en-GB', { hour: '2-digit', minute: '2-digit' });
}

function totalCoins(s)  { return s.levels.reduce((a, l) => a + (l.attempted ? l.coins : 0), 0); }
function totalErrors(s) { return s.levels.reduce((a, l) => a + (l.attempted ? l.incorrectDishes + l.duplicateIngredientClicks + l.glutenChildServedByMistake : 0), 0); }
function totalGluten(s) { return s.levels.reduce((a, l) => a + (l.attempted ? l.glutenChildServedByMistake : 0), 0); }
function levelsPassed(s){ return s.levels.filter(l => l.attempted && l.passed).length; }

function avgTimeForSession(s) {
  const times = s.levels.filter(l => l.attempted && l.timeToTargetSeconds !== -1).map(l => l.timeToTargetSeconds);
  return times.length ? Math.round(times.reduce((a, b) => a + b, 0) / times.length) : -1;
}

function lastLevelPassed(sessions) {
  let max = 0;
  for (const s of sessions)
    for (const l of s.levels)
      if (l.attempted && l.passed && l.levelNumber > max) max = l.levelNumber;
  return max;
}

function totalServedAll(sessions) {
  return sessions.reduce((a, s) => a + s.levels.reduce((b, l) => b + (l.attempted ? l.totalServedDishes : 0), 0), 0);
}

function shadeClass(val, low, high) {
  if (val === -1 || val === null || val === undefined) return '';
  if (val <= low)  return 'shade-low';
  if (val <= high) return 'shade-mid';
  return 'shade-high';
}

// ── Data filtering & sorting ──────────────────────────────────────────────────
function filteredSessions() {
  let sessions = (SABABICH_DATA.sessions || []).filter(s => {
    if (selectedPatient && s.displayName !== selectedPatient) return false;
    if (filterDateFrom && s.sessionDateTimeISO < filterDateFrom) return false;
    if (filterDateTo   && s.sessionDateTimeISO > filterDateTo + 'T23:59:59') return false;
    if (filterLevel > 0 && !s.levels.some(l => l.levelNumber === filterLevel && l.attempted)) return false;
    if (filterResult === 'pass' && levelsPassed(s) === 0) return false;
    if (filterResult === 'fail' && levelsPassed(s) > 0)  return false;
    return true;
  });

  sessions = sessions.slice().sort((a, b) => {
    switch (sortMode) {
      case 'oldest':    return a.sessionDateTimeISO.localeCompare(b.sessionDateTimeISO);
      case 'coinsHigh': return totalCoins(b) - totalCoins(a);
      case 'coinsLow':  return totalCoins(a) - totalCoins(b);
      case 'time':      return avgTimeForSession(a) - avgTimeForSession(b);
      case 'duplicates':
        return b.levels.reduce((s, l) => s + l.duplicateIngredientClicks, 0) -
               a.levels.reduce((s, l) => s + l.duplicateIngredientClicks, 0);
      case 'gluten':    return totalGluten(b) - totalGluten(a);
      case 'perfect': {
        const rate = s => {
          const t = s.levels.reduce((a, l) => a + l.totalServedDishes, 0);
          return t ? s.levels.reduce((a, l) => a + l.perfectServedDishes, 0) / t : 0;
        };
        return rate(b) - rate(a);
      }
      default: return b.sessionDateTimeISO.localeCompare(a.sessionDateTimeISO);
    }
  });

  return sessions;
}

// ── Render ────────────────────────────────────────────────────────────────────
function render() {
  renderLangToggle();
  renderPatientSelector();
  renderFilterLabels();
  renderCards();
  renderSessionList();
  renderDetail();
  renderCharts();
}

function renderLangToggle() {
  document.documentElement.setAttribute('dir', lang === 'he' ? 'rtl' : 'ltr');
  document.querySelector('.header h1').textContent   = L().title;
  document.querySelector('.lang-btn').textContent    = L().langBtn;
  document.querySelector('.filters-toggle').textContent = L().filtersToggle;
  document.querySelector('.filters-reset').textContent  = L().resetFilters;
}

function renderPatientSelector() {
  const sel = document.getElementById('patientSelect');
  const all = [...new Set((SABABICH_DATA.sessions || []).map(s => s.displayName))].sort();
  sel.innerHTML = `<option value="">${L().patientAll}</option>` +
    all.map(n => `<option value="${n}" ${n === selectedPatient ? 'selected' : ''}>${n}</option>`).join('');
  document.querySelector('.patient-bar label').textContent = L().patientLabel;
}

function renderFilterLabels() {
  document.querySelectorAll('[data-label]').forEach(el => {
    el.textContent = L()[el.dataset.label] || '';
  });
}

function renderCards() {
  const sessions  = filteredSessions();
  const allLevels = sessions.flatMap(s => s.levels.filter(l => l.attempted));
  const avgCoins  = allLevels.length
    ? Math.round(allLevels.reduce((a, l) => a + l.coins, 0) / allLevels.length) : 0;
  // Avg dish prep time: total time-to-target ÷ total perfect dishes across all levels
  const levelsWithData = allLevels.filter(l => l.timeToTargetSeconds !== -1 && l.perfectServedDishes > 0);
  const totalTime      = levelsWithData.reduce((a, l) => a + l.timeToTargetSeconds, 0);
  const totalPerfect   = levelsWithData.reduce((a, l) => a + l.perfectServedDishes, 0);
  const avgPrepTime    = totalPerfect > 0 ? (totalTime / totalPerfect).toFixed(1) : -1;

  const llp = lastLevelPassed(sessions);

  document.getElementById('cardSessions').textContent    = sessions.length;
  document.getElementById('cardPassed').textContent      = llp > 0 ? llp : L().na;
  document.getElementById('cardAvgCoins').textContent    = avgCoins;
  document.getElementById('cardAvgTime').textContent     = avgPrepTime === -1 ? L().na : avgPrepTime;
  document.getElementById('cardTotalServed').textContent = totalServedAll(sessions);
  document.getElementById('cardErrors').textContent      = sessions.reduce((a, s) => a + totalErrors(s), 0);

  document.getElementById('labelSessions').textContent   = L().cardSessions;
  document.getElementById('labelPassed').textContent     = L().cardPassed;
  document.getElementById('labelAvgCoins').textContent   = L().cardAvgCoins;
  document.getElementById('labelAvgTime').textContent    = L().cardAvgTime;
  document.getElementById('labelTotalServed').textContent = L().cardTotalServed;
  document.getElementById('labelErrors').textContent     = L().cardErrors;
}

function renderSessionList() {
  const sessions = filteredSessions();
  document.getElementById('sessionTitle').textContent = L().sessionTitle;
  document.getElementById('sortLabel').textContent    = L().sortLabel;

  const container = document.getElementById('sessionList');
  if (sessions.length === 0) {
    container.innerHTML = `<p style="color:#aaa;font-size:0.9rem;padding:10px 0">${lang === 'he' ? 'אין סשנים להצגה' : 'No sessions to display'}</p>`;
    return;
  }

  container.innerHTML = sessions.map(s => {
    const active = selectedSession && selectedSession.sessionId === s.sessionId ? 'active' : '';
    const badge  = s.isGuest
      ? `<span class="badge badge-guest">${L().metaGuest}</span>`
      : levelsPassed(s) > 0
        ? `<span class="badge badge-pass">${L().pass}</span>`
        : `<span class="badge badge-fail">${L().fail}</span>`;
    return `<div class="session-row ${active}" onclick="selectSession('${s.sessionId}')">
      <span class="session-name">${s.displayName}</span>
      <span class="session-date">${fmtDate(s.sessionDateTimeISO)}</span>
      ${badge}
      <span style="font-size:0.82rem;color:#7a8a9e">${lang === 'he' ? 'מטבעות' : 'Coins'}: ${totalCoins(s)}</span>
    </div>`;
  }).join('');
}

function renderDetail() {
  const panel = document.getElementById('detailPanel');
  document.getElementById('detailTitle').textContent = L().detailTitle;
  if (!selectedSession) { panel.classList.remove('open'); return; }
  panel.classList.add('open');

  const s = selectedSession;
  document.getElementById('detailMeta').innerHTML =
    `<span>${L().metaDate} ${fmtDate(s.sessionDateTimeISO)}</span>` +
    `<span>${L().metaScene} ${s.resumeScene || '—'}</span>` +
    (s.isGuest ? `<span class="badge badge-guest">${L().metaGuest}</span>` : '');

  renderLevelTable(s);
  renderObsSections(s);
  renderNotes(s);
}

function renderLevelTable(s) {
  const T = L();
  const headers = [T.tableLevel, T.tableResult, T.tableCoins, T.tableTime,
    T.tableTotal, T.tablePerfect, T.tableIncorrect, T.tableDuplicate,
    T.tableGlutenApp, T.tableGlutenServed, T.tableGlutenHandled, T.tableAvgPrep];

  const rows = s.levels.map(l => {
    if (!l.attempted) {
      return `<tr><td>${l.levelNumber}</td><td colspan="11" class="na">${T.notAttempted}</td></tr>`;
    }
    const res = l.passed
      ? `<span class="badge badge-pass">${T.pass}</span>`
      : `<span class="badge badge-fail">${T.fail}</span>`;
    const avg = l.averageDishPrepTimeSeconds === -1
      ? `<span class="na">${T.na}</span>`
      : l.averageDishPrepTimeSeconds.toFixed(1);
    return `<tr>
      <td>${l.levelNumber}</td>
      <td>${res}</td>
      <td class="${shadeClass(l.coins, 50, 150)}">${l.coins}</td>
      <td class="${shadeClass(l.timeToTargetSeconds, 30, 80)}">${fmt(l.timeToTargetSeconds)}</td>
      <td>${l.totalServedDishes}</td>
      <td>${l.perfectServedDishes}</td>
      <td class="${shadeClass(l.incorrectDishes, 1, 3)}">${l.incorrectDishes}</td>
      <td class="${shadeClass(l.duplicateIngredientClicks, 1, 3)}">${l.duplicateIngredientClicks}</td>
      <td>${l.glutenChildAppeared}</td>
      <td class="${shadeClass(l.glutenChildServedByMistake, 0, 1)}">${l.glutenChildServedByMistake}</td>
      <td>${l.glutenChildHandledCorrectly}</td>
      <td>${avg}</td>
    </tr>`;
  });

  document.getElementById('levelTable').innerHTML =
    `<thead><tr>${headers.map(h => `<th>${h}</th>`).join('')}</tr></thead>` +
    `<tbody>${rows.join('')}</tbody>`;
}

function renderObsSections(s) {
  const T = L();
  const attempted = s.levels.filter(l => l.attempted);
  if (attempted.length === 0) { document.getElementById('obsSections').innerHTML = ''; return; }

  const levelNums = attempted.map(l => `<th>${T.tableLevel} ${l.levelNumber}</th>`).join('');

  const mkRow = (label, vals) =>
    `<tr><td style="text-align:start;font-weight:600;padding:6px 10px">${label}</td>` +
    vals.map(v => `<td>${v}</td>`).join('') + '</tr>';

  const section = (title, rows) =>
    `<div class="obs-section"><div class="obs-section-title">${title}</div>` +
    `<div class="level-table-wrap"><table><thead><tr><th></th>${levelNums}</tr></thead>` +
    `<tbody>${rows}</tbody></table></div></div>`;

  document.getElementById('obsSections').innerHTML =
    section(T.obsDishes,
      mkRow(T.tableTotal,    attempted.map(l => l.totalServedDishes)) +
      mkRow(T.tablePerfect,  attempted.map(l => l.perfectServedDishes)) +
      mkRow(T.tableIncorrect,attempted.map(l => l.incorrectDishes))
    ) +
    section(T.obsClicks,
      mkRow(T.tableDuplicate, attempted.map(l => l.duplicateIngredientClicks))
    ) +
    section(T.obsGluten,
      mkRow(T.tableGlutenApp,     attempted.map(l => l.glutenChildAppeared)) +
      mkRow(T.tableGlutenServed,  attempted.map(l => l.glutenChildServedByMistake)) +
      mkRow(T.tableGlutenHandled, attempted.map(l => l.glutenChildHandledCorrectly))
    ) +
    section(T.obsOutcome,
      mkRow(T.tableCoins,  attempted.map(l => l.coins)) +
      mkRow(T.tableResult, attempted.map(l => l.passed
        ? `<span class="badge badge-pass">${T.pass}</span>`
        : `<span class="badge badge-fail">${T.fail}</span>`))
    ) +
    section(T.obsPlanning,
      mkRow(T.tableTime,    attempted.map(l => fmt(l.timeToTargetSeconds))) +
      mkRow(T.tableAvgPrep, attempted.map(l =>
        l.averageDishPrepTimeSeconds === -1
          ? `<span class="na">${T.na}</span>`
          : l.averageDishPrepTimeSeconds.toFixed(1)))
    );
}

function renderNotes(s) {
  const T = L();
  document.getElementById('notesTitle').textContent       = T.notesTitle;
  document.getElementById('notesArea').placeholder        = T.notesPlaceholder;
  document.getElementById('printBtn').textContent         = T.printBtn;
  const key   = `sababich_note_${s.sessionId}`;
  const saved = localStorage.getItem(key) || '';
  document.getElementById('notesArea').value              = saved;
  document.getElementById('notesMeta').textContent        = saved ? T.notesSaved : '';
}

function renderCharts() {
  if (typeof Chart === 'undefined') return;

  const sessions = filteredSessions().slice().reverse(); // oldest first for trend lines
  document.getElementById('chartsTitle').textContent          = L().chartsTitle;
  document.getElementById('labelChartCoins').textContent      = L().chartCoins;
  document.getElementById('labelChartErrors').textContent     = L().chartErrors;
  const elPerfect = document.getElementById('labelChartPerfect');
  const elTime    = document.getElementById('labelChartTime');
  const elGluten  = document.getElementById('labelChartGluten');
  if (elPerfect) elPerfect.textContent = L().chartPerfect;
  if (elTime)    elTime.textContent    = L().chartTime;
  if (elGluten)  elGluten.textContent  = L().chartGluten;

  const labels = sessions.map(s => fmtDate(s.sessionDateTimeISO));

  if (coinsChart) coinsChart.destroy();
  coinsChart = new Chart(document.getElementById('chartCoins').getContext('2d'), {
    type: 'line',
    data: {
      labels,
      datasets: [1, 2, 3].map(n => ({
        label: `${lang === 'he' ? 'רמה' : 'Level'} ${n}`,
        data: sessions.map(s => { const l = s.levels.find(l => l.levelNumber === n); return l && l.attempted ? l.coins : null; }),
        spanGaps: true,
        tension: 0.3,
        pointRadius: 4,
      }))
    },
    options: { responsive: true, plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
  });

  if (errorsChart) errorsChart.destroy();
  errorsChart = new Chart(document.getElementById('chartErrors').getContext('2d'), {
    type: 'bar',
    data: {
      labels,
      datasets: [{
        label: lang === 'he' ? 'שגיאות' : 'Errors',
        data: sessions.map(s => totalErrors(s)),
        backgroundColor: '#7aaad4',
      }]
    },
    options: { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
  });

  const perfectCanvas = document.getElementById('chartPerfect');
  if (perfectCanvas) {
    if (perfectChart) perfectChart.destroy();
    perfectChart = new Chart(perfectCanvas.getContext('2d'), {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: lang === 'he' ? 'מושלם' : 'Perfect',
            data: sessions.map(s => s.levels.reduce((a, l) => a + (l.attempted ? l.perfectServedDishes : 0), 0)),
            backgroundColor: '#a8d5b5',
          },
          {
            label: lang === 'he' ? 'שגוי' : 'Incorrect',
            data: sessions.map(s => s.levels.reduce((a, l) => a + (l.attempted ? l.incorrectDishes : 0), 0)),
            backgroundColor: '#f4a6a6',
          }
        ]
      },
      options: { responsive: true, plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
    });
  }

  const timeCanvas = document.getElementById('chartTime');
  if (timeCanvas) {
    if (timeChart) timeChart.destroy();
    timeChart = new Chart(timeCanvas.getContext('2d'), {
      type: 'line',
      data: {
        labels,
        datasets: [1, 2, 3].map(n => ({
          label: `${lang === 'he' ? 'רמה' : 'Level'} ${n}`,
          data: sessions.map(s => {
            const l = s.levels.find(l => l.levelNumber === n);
            return l && l.attempted && l.timeToTargetSeconds !== -1 ? l.timeToTargetSeconds : null;
          }),
          spanGaps: true,
          tension: 0.3,
          pointRadius: 4,
        }))
      },
      options: { responsive: true, plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
    });
  }

  const glutenCanvas = document.getElementById('chartGluten');
  if (glutenCanvas) {
    if (glutenChart) glutenChart.destroy();
    glutenChart = new Chart(glutenCanvas.getContext('2d'), {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: lang === 'he' ? 'הופיע' : 'Appeared',
            data: sessions.map(s => s.levels.reduce((a, l) => a + (l.attempted ? l.glutenChildAppeared : 0), 0)),
            backgroundColor: '#b0c4de',
          },
          {
            label: lang === 'he' ? 'הוגש בטעות' : 'Served by Mistake',
            data: sessions.map(s => s.levels.reduce((a, l) => a + (l.attempted ? l.glutenChildServedByMistake : 0), 0)),
            backgroundColor: '#f4a6a6',
          }
        ]
      },
      options: { responsive: true, plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
    });
  }
}

// ── Event handlers ────────────────────────────────────────────────────────────
function selectSession(id) {
  const s = (SABABICH_DATA.sessions || []).find(s => s.sessionId === id);
  selectedSession = (selectedSession && selectedSession.sessionId === id) ? null : s;
  render();
}

document.getElementById('patientSelect').addEventListener('change', e => {
  selectedPatient = e.target.value;
  selectedSession = null;
  render();
});

document.getElementById('sortSelect').addEventListener('change', e => {
  sortMode = e.target.value;
  render();
});

document.querySelector('.lang-btn').addEventListener('click', () => {
  lang = lang === 'he' ? 'en' : 'he';
  render();
});

document.querySelector('.filters-toggle').addEventListener('click', () => {
  document.querySelector('.filters-panel').classList.toggle('open');
});

document.querySelector('.filters-reset').addEventListener('click', () => {
  filterDateFrom = filterDateTo = '';
  filterLevel = 0;
  filterResult = 'all';
  document.getElementById('fDateFrom').value = '';
  document.getElementById('fDateTo').value   = '';
  document.getElementById('fLevel').value    = '0';
  document.getElementById('fResult').value   = 'all';
  selectedSession = null;
  render();
});

document.getElementById('fDateFrom').addEventListener('change', e => { filterDateFrom = e.target.value; render(); });
document.getElementById('fDateTo').addEventListener('change',   e => { filterDateTo   = e.target.value; render(); });
document.getElementById('fLevel').addEventListener('change',    e => { filterLevel    = Number(e.target.value); render(); });
document.getElementById('fResult').addEventListener('change',   e => { filterResult   = e.target.value; render(); });

document.getElementById('notesArea').addEventListener('input', e => {
  if (!selectedSession) return;
  const key = `sababich_note_${selectedSession.sessionId}`;
  localStorage.setItem(key, e.target.value);
  document.getElementById('notesMeta').textContent = L().notesSaved;
});

document.getElementById('printBtn').addEventListener('click', () => window.print());

// ── Init ──────────────────────────────────────────────────────────────────────
render();
