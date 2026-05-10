// ── API ───────────────────────────────────────────────────────────────────────

const API_BASE = "/api";
const USERNAME = "brian";

async function apiFetch(url, options = {}) {
  try {
    const res = await fetch(API_BASE + url, options);
    if (!res.ok) return null;
    if (res.status === 204) return true;
    return await res.json();
  } catch(e) {
    console.error("API error:", e);
    return null;
  }
}

// ── Progress ──────────────────────────────────────────────────────────────────

let progress = {};

async function loadProgress() {
  const data = await apiFetch(`/progress/${USERNAME}`);
  if (!data) return;
  data.forEach(entry => {
    const k = "w" + entry.week;
    if (!progress[k]) progress[k] = {};
    progress[k][entry.day] = true;
  });
}

async function markDayComplete(week, day) {
  await apiFetch(`/progress/${USERNAME}/${week}/${day}`, { method: "POST" });
  const k = "w" + week;
  if (!progress[k]) progress[k] = {};
  progress[k][day] = true;
}

async function unmarkDay(week, day) {
  await apiFetch(`/progress/${USERNAME}/${week}/${day}`, { method: "DELETE" });
  const k = "w" + week;
  if (progress[k]) progress[k][day] = false;
}

function isDayDone(week, day) { return !!(progress["w" + week]?.[day]); }
function isWeekDone(week) {
  const wk = progress["w" + week];
  return !!(wk?.A && wk?.B && wk?.C);
}

// ── Exercises ─────────────────────────────────────────────────────────────────

// Cache exercises per day so we don't re-fetch on every render
const exerciseCache = {};

async function loadExercisesForDay(day) {
  if (exerciseCache[day]) return exerciseCache[day];
  const data = await apiFetch(`/exercises/day/${day}`);
  if (data) exerciseCache[day] = data;
  return data || [];
}

async function getSuggestion(exerciseId, week) {
  return await apiFetch(`/exercises/${exerciseId}/suggestion/${USERNAME}/${week}`);
}

// ── Program Data ──────────────────────────────────────────────────────────────

const PHASES = [
  { id: 1, name: "Base",     weeks: "1\u201313",  weekRange: [1,13],  color: "#a8d8a8", accent: "#4a9e4a",
    goal: "Build aerobic base, establish movement patterns, begin cutting fat",
    description: "Light weights, higher reps. Lock in form and consistency. Cardio is your primary fat-loss driver." },
  { id: 2, name: "Strength", weeks: "14\u201326", weekRange: [14,26], color: "#a8c8e8", accent: "#2a7ab8",
    goal: "Build lean muscle, accelerate fat loss, improve overall conditioning",
    description: "Moderate weights, mixed rep ranges. Supersets introduced. Cardio intensity steps up." },
  { id: 3, name: "Cut",      weeks: "27\u201339", weekRange: [27,39], color: "#d8a8e8", accent: "#8a2ab8",
    goal: "Maximize muscle definition \u2014 V-taper, posterior chain, core cuts",
    description: "Heavier loads on key lifts, drop sets introduced. Cardio shifts to intervals for maximum fat burn." },
  { id: 4, name: "Peak",     weeks: "40\u201352", weekRange: [40,52], color: "#e8c8a8", accent: "#b87a2a",
    goal: "Peak conditioning \u2014 full body definition, hold target weight",
    description: "Progressive overload peaks. Lean mass preserved while body fat hits its floor. Mixed cardio protocol." },
];

const DAY_COLORS  = { A: "#e8a8b8", B: "#a8d8a8", C: "#a8c8e8" };
const DAY_ICONS   = { A: "💪", B: "🦵", C: "🏋️" };

let selectedWeek = 1;
let selectedDay  = "A";
let expandedEx   = null;

// ── Helpers ───────────────────────────────────────────────────────────────────

function getPhase(week) { return PHASES.find(p => week >= p.weekRange[0] && week <= p.weekRange[1]); }

function weightScale(week) {
  if (week <= 13) return 1.0   + (week-1)  * 0.02;
  if (week <= 26) return 1.26  + (week-14) * 0.025;
  if (week <= 39) return 1.575 + (week-27) * 0.03;
  return 1.965 + (week-40) * 0.02;
}

function scaleWeight(base, week, step=5) {
  return Math.max(base, Math.round((base * weightScale(week)) / step) * step);
}

function googleImagesUrl(name) {
  return "https://www.google.com/search?tbm=isch&q=" + encodeURIComponent(name + " exercise form");
}

function youtubeUrl(name) {
  return "https://www.youtube.com/results?search_query=" + encodeURIComponent(name + " how to tutorial");
}

// ── Cardio ────────────────────────────────────────────────────────────────────

function cardioForWeek(week) {
  if (week <= 13) {
    const mins = Math.min(20 + Math.floor((week-1)/2)*2, 30);
    return { machine:"Treadmill or Elliptical", duration:mins+" min",
      intensity:"Moderate \u2014 60\u201365% max heart rate",
      notes:"Steady aerobic pace. Brisk walk or light jog. This is your fat-burning foundation \u2014 don't skip it." };
  }
  if (week <= 26) {
    const mins = Math.min(30 + Math.floor((week-14)/3)*2, 40);
    return { machine:"Elliptical or Stair Stepper", duration:mins+" min",
      intensity:"Moderate-High \u2014 65\u201370% max heart rate",
      notes:"Bump resistance every 3 weeks. Stair stepper is a powerful posterior chain and cardio tool \u2014 lean slightly forward." };
  }
  if (week <= 39) {
    const n = Math.min(6 + Math.floor((week-27)/3), 10);
    return { machine:"Treadmill (intervals) + Elliptical (steady)", duration:"35 min total",
      intensity:"Intervals: 80\u201385% / Steady: 65\u201370%",
      notes:n+" x 60-sec sprint / 90-sec walk on treadmill, then 15 min steady elliptical. Maximizes fat oxidation." };
  }
  return { machine:"Mixed \u2014 Treadmill, Stair Stepper, Elliptical", duration:"45 min total",
    intensity:"Varied \u2014 interval + steady-state",
    notes:"15 min HIIT treadmill + 15 min stair stepper + 15 min elliptical cool-down. Rotate machine order each session." };
}

// ── Workout data (hardcoded structure, weights come from API suggestions) ─────

function getWorkoutStructure(week) {
  const ph  = week<=13?1:week<=26?2:week<=39?3:4;
  const s34 = ph>=3?4:3;
  const rp  = ph===1?"15":ph===2?"12":ph===3?"10":"8\u201310";
  const cr  = ph===1?"20":"15\u201320";

  return {
    A: { label:"Day A \u2014 Upper Body + Core", focus:"Chest, Shoulders, Triceps, Core",
      exercises:[
        { exerciseId:1,  sets:s34, reps:rp, target:"Upper chest \u2014 builds shelf and pec definition",         notes:"Set bench to 30\u201345\u00b0. Vertical Smith is excellent here. Slow 3-sec descent, press straight up. Upper chest development gives the pecs a defined, separated look." },
        { exerciseId:2,  sets:3,   reps:rp, target:"Mid chest \u2014 overall pec mass and thickness",           notes:"Flat bench under the bar. Lower to mid-chest, press without locking elbows. Full range of motion for maximum pec recruitment." },
        { exerciseId:3,  sets:3,   reps:ph===1?"15":"12", target:"Inner chest \u2014 pec separation and cuts",   notes:"Squeeze hard at peak contraction and hold 1 second. Isolates the sternal head of the pec \u2014 the key to a defined, cut chest." },
        { exerciseId:4,  sets:s34, reps:rp, target:"Shoulders \u2014 width and overhead strength",              notes:"Seated. Press straight up, 2\u20133 sec controlled descent. Broad shoulders anchor the V-taper and make the waist look tighter by comparison." },
        { exerciseId:5,  sets:3,   reps:"15", target:"Rear deltoids \u2014 3D shoulder look and posture",       notes:"Face the machine, palms in, pull arms wide. Rear delt development rounds out the shoulder from every angle and straightens posture." },
        { exerciseId:6,  sets:3,   reps:rp, target:"Triceps \u2014 arm size and horseshoe definition",          notes:"Full lockout at the bottom, controlled 2\u20133 sec return. Triceps make up two-thirds of arm size \u2014 key for lean, defined arms." },
        { exerciseId:7,  sets:3,   reps:cr, target:"Rectus abdominis \u2014 six-pack development",              notes:"Flex the abs hard at the bottom, slow return. No swinging \u2014 the abs do the work, not momentum." },
        { exerciseId:8,  sets:3,   reps:"20 each side", target:"Obliques \u2014 tight, defined midsection",     notes:"Moderate weight only \u2014 the goal is a tight, narrow waist. Rotate through the torso, keep hips planted." },
      ]},
    B: { label:"Day B \u2014 Lower Body", focus:"Glutes, Hamstrings, Quads, Calves",
      exercises:[
        { exerciseId:9,  sets:s34, reps:rp, target:"Quads, glutes, hamstrings \u2014 compound lower body strength", notes:"Feet slightly forward of the bar \u2014 accounts for the vertical bar path. Drive through heels, hit parallel or below for full posterior chain activation." },
        { exerciseId:10, sets:s34, reps:rp, target:"Glutes and hamstrings \u2014 posterior chain power",            notes:"Upper back on bench, bar padded across hips. Drive straight up, hard glute squeeze at the top, hold 1 second. The vertical Smith is ideal for this. Best posterior chain builder in the program." },
        { exerciseId:11, sets:3,   reps:rp, target:"Hamstrings and glutes \u2014 posterior chain length",          notes:"Hip hinge, slight knee bend, bar tracks close to legs. Full hamstring stretch at the bottom. Keep the lower back flat throughout." },
        { exerciseId:12, sets:3,   reps:rp, target:"Quads and glutes \u2014 leg mass and pressing power",          notes:"High, wide foot placement shifts load to glutes and hamstrings. Don't lock knees at the top. Deep range of motion for full muscle recruitment." },
        { exerciseId:13, sets:3,   reps:rp, target:"Hamstrings \u2014 isolation and definition",                  notes:"Full range of motion, 3-sec eccentric on the return. Isolated hamstring work adds thickness and definition to the back of the legs." },
        { exerciseId:14, sets:3,   reps:"20", target:"Gluteus medius \u2014 outer hip and glute width",           notes:"Drive knees outward against the pads. Higher reps hit the gluteus medius hard \u2014 builds the outer sweep of the hips and glutes." },
        { exerciseId:15, sets:3,   reps:"20", target:"Inner thighs \u2014 leg definition and stability",          notes:"Controlled squeeze of the knees together. Develops the adductors for complete leg definition and joint stability." },
        { exerciseId:16, sets:3,   reps:"20", target:"Gastrocnemius and soleus \u2014 lower leg development",     notes:"Full range \u2014 max stretch at the bottom, full contraction at the top, 1-second hold. Calves respond best to high reps and full ROM." },
      ]},
    C: { label:"Day C \u2014 Back, Biceps + Core", focus:"Back, Biceps, Obliques, Core",
      exercises:[
        { exerciseId:17, sets:s34, reps:rp, target:"Lats \u2014 V-taper width and back thickness",               notes:"Wide overhand grip, bar pulled to upper chest, elbows drive down toward hips. Wide lats are the foundation of the V-taper \u2014 top priority exercise on Day C." },
        { exerciseId:18, sets:3,   reps:rp, target:"Mid back and rhomboids \u2014 back thickness and posture",   notes:"Chest up, sit tall. Pull to midsection, hard squeeze of the shoulder blades. Back thickness makes the physique look powerful from every angle." },
        { exerciseId:19, sets:3,   reps:rp, target:"Upper back \u2014 overall back density",                     notes:"45\u00b0 hinge, bar pulled to lower chest, back stays flat. Focus on the contraction at the top. Adds density across the entire upper back." },
        { exerciseId:20, sets:3,   reps:rp, target:"Biceps \u2014 arm size and peak",                            notes:"Full range \u2014 all the way down, squeeze hard at the top. Slow 2\u20133 sec descent. Machine keeps constant tension for maximum bicep stimulus." },
        { exerciseId:21, sets:3,   reps:ph===1?"10\u201312":"8\u201310", target:"Lats and biceps \u2014 upper body pulling strength", notes:"Start with high counterweight (more assist = easier), reduce it steadily over the weeks. Overhand grip, pull chest toward the bar. One of the best compound upper body movements." },
        { exerciseId:22, sets:3,   reps:"20 each side", target:"Obliques \u2014 core definition and midsection tightness", notes:"Smooth rotation through the torso, hips stay square. Moderate weight \u2014 the goal is definition and a tight waist, not oblique bulk." },
        { exerciseId:23, sets:3,   reps:cr, target:"Core \u2014 abdominal strength and definition",              notes:"Hard flex at the bottom, hold 1 second, controlled return. Ab definition becomes visible as body fat drops through consistent cardio and diet." },
      ]},
  };
}

// ── Exercise name lookup from cache ───────────────────────────────────────────

function getExerciseName(exerciseId) {
  for (const day of ["A","B","C"]) {
    if (!exerciseCache[day]) continue;
    const found = exerciseCache[day].find(e => e.exercise.id === exerciseId);
    if (found) return found.exercise.name;
  }
  return "Exercise " + exerciseId;
}

// ── Session Logging Screen ────────────────────────────────────────────────────

let sessionLogData = {}; // exerciseId -> { actualWeight, completed, actualReps }

async function showLoggingScreen(week, day) {
  const phase    = getPhase(week);
  const structure = getWorkoutStructure(week);
  const workout   = structure[day];

  // Pre-load exercises for this day if not cached
  await loadExercisesForDay(day);

  // Fetch suggestions for all exercises in parallel
  const suggestionPromises = workout.exercises.map(ex =>
    getSuggestion(ex.exerciseId, week).then(s => ({ exerciseId: ex.exerciseId, suggestion: s }))
  );
  const suggestions = await Promise.all(suggestionPromises);
  const suggestionMap = {};
  suggestions.forEach(s => {
    if (s.suggestion) suggestionMap[s.exerciseId] = s.suggestion;
  });

  // Reset session log data
  sessionLogData = {};
  workout.exercises.forEach(ex => {
    const suggested = suggestionMap[ex.exerciseId]?.suggestedWeightLbs ?? 0;
    sessionLogData[ex.exerciseId] = {
      actualWeight: suggested,
      completed: null, // null = not yet selected
      actualReps: null,
      prescribedWeight: suggested,
      prescribedSets: ex.sets,
      prescribedReps: ex.reps
    };
  });

  // Build the logging screen HTML
  const overlay = document.createElement("div");
  overlay.id = "logging-overlay";

  let html = `
    <div class="log-screen">
      <div class="log-header" style="border-color:${phase.accent}">
        <div class="log-title">${DAY_ICONS[day]} Log Session — Week ${week} Day ${day}</div>
        <div class="log-subtitle">Enter the weight you used and rate each exercise</div>
      </div>
      <div class="log-exercises" id="log-exercise-list">
  `;

  workout.exercises.forEach((ex, i) => {
    const name      = getExerciseName(ex.exerciseId);
    const suggested = suggestionMap[ex.exerciseId]?.suggestedWeightLbs ?? 0;
    const source    = suggestionMap[ex.exerciseId]?.source ?? "formula";
    const isAssist  = ex.exerciseId === 21; // Assisted Pull-Up — weight means assistance

    html += `
      <div class="log-ex-card" id="log-ex-${ex.exerciseId}">
        <div class="log-ex-header">
          <div class="log-ex-num">${i + 1}</div>
          <div class="log-ex-info">
            <div class="log-ex-name">${name}</div>
            <div class="log-ex-meta">${ex.sets} sets · ${ex.reps} reps</div>
          </div>
          <div class="log-ex-suggested">
            <div class="log-ex-suggested-val">${suggested} lbs${isAssist ? " assist" : ""}</div>
            <div class="log-ex-suggested-label">${source === "progression" ? "📈 progression" : "📋 formula"}</div>
          </div>
        </div>
        <div class="log-ex-inputs">
          <div class="log-weight-row">
            <label class="log-label">Actual weight (lbs${isAssist ? " assist" : ""})</label>
            <input
              type="number"
              class="log-weight-input"
              id="weight-${ex.exerciseId}"
              value="${suggested}"
              min="0"
              step="5"
              onchange="updateSessionLog(${ex.exerciseId}, 'weight', this.value)"
            />
          </div>
          <div class="log-thumb-row">
            <button class="log-thumb-btn thumb-up" id="up-${ex.exerciseId}"
              onclick="setCompleted(${ex.exerciseId}, true)">👍 Completed</button>
            <button class="log-thumb-btn thumb-down" id="down-${ex.exerciseId}"
              onclick="setCompleted(${ex.exerciseId}, false)">👎 Struggled</button>
          </div>
          <div class="log-reps-row hidden" id="reps-row-${ex.exerciseId}">
            <label class="log-label">Actual reps completed</label>
            <input
              type="number"
              class="log-reps-input"
              id="reps-${ex.exerciseId}"
              min="0"
              placeholder="e.g. 10"
              onchange="updateSessionLog(${ex.exerciseId}, 'reps', this.value)"
            />
          </div>
        </div>
      </div>
    `;
  });

  html += `
      </div>
      <div class="log-actions">
        <button class="log-cancel-btn" onclick="closeLoggingScreen()">Cancel</button>
        <button class="log-save-btn" id="log-save-btn" style="background:${phase.accent}"
          onclick="saveSession(${week}, '${day}')">Save Session &amp; Mark Done</button>
      </div>
    </div>
  `;

  overlay.innerHTML = html;
  document.body.appendChild(overlay);
}

function updateSessionLog(exerciseId, field, value) {
  if (!sessionLogData[exerciseId]) return;
  if (field === "weight") sessionLogData[exerciseId].actualWeight = parseFloat(value) || 0;
  if (field === "reps")   sessionLogData[exerciseId].actualReps   = parseInt(value)   || null;
}

function setCompleted(exerciseId, completed) {
  if (!sessionLogData[exerciseId]) return;
  sessionLogData[exerciseId].completed = completed;

  const upBtn   = document.getElementById("up-"   + exerciseId);
  const downBtn = document.getElementById("down-" + exerciseId);
  const repsRow = document.getElementById("reps-row-" + exerciseId);

  upBtn.classList.toggle("active",  completed);
  downBtn.classList.toggle("active", !completed);
  repsRow.classList.toggle("hidden", completed);
}

function closeLoggingScreen() {
  const overlay = document.getElementById("logging-overlay");
  if (overlay) overlay.remove();
}

async function saveSession(week, day) {
  const saveBtn = document.getElementById("log-save-btn");
  saveBtn.textContent = "Saving...";
  saveBtn.disabled = true;

  // Validate all exercises have been rated
  const structure = getWorkoutStructure(week);
  const workout   = structure[day];
  const unrated   = workout.exercises.filter(ex => sessionLogData[ex.exerciseId]?.completed === null);

  if (unrated.length > 0) {
    alert(`Please rate all exercises before saving. ${unrated.length} exercise(s) still need a 👍 or 👎.`);
    saveBtn.textContent = "Save Session & Mark Done";
    saveBtn.disabled = false;
    return;
  }

  // Build request payload
  const payload = workout.exercises.map(ex => ({
    exerciseId:          ex.exerciseId,
    prescribedWeightLbs: sessionLogData[ex.exerciseId].prescribedWeight,
    actualWeightLbs:     sessionLogData[ex.exerciseId].actualWeight,
    prescribedSets:      ex.sets,
    prescribedReps:      ex.reps,
    completed:           sessionLogData[ex.exerciseId].completed,
    actualReps:          sessionLogData[ex.exerciseId].actualReps
  }));

  // Save session log
  const result = await apiFetch(`/sessions/${USERNAME}/${week}/${day}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!result) {
    alert("Failed to save session. Please try again.");
    saveBtn.textContent = "Save Session & Mark Done";
    saveBtn.disabled = false;
    return;
  }

  // Mark day complete
  await markDayComplete(week, day);

  closeLoggingScreen();
  render();
}

// ── Main Render ───────────────────────────────────────────────────────────────

function render() {
  const phase    = getPhase(selectedWeek);
  const structure = getWorkoutStructure(selectedWeek);
  const cardio   = cardioForWeek(selectedWeek);
  const workout  = structure[selectedDay];

  // Phase banner
  document.getElementById("phase-banner").style.borderColor = phase.accent;
  const tag = document.getElementById("phase-tag");
  tag.textContent      = "Phase " + phase.id;
  tag.style.background = phase.accent;
  document.getElementById("phase-name").textContent  = phase.name;
  document.getElementById("phase-weeks").textContent = "Weeks " + phase.weeks;
  document.getElementById("phase-goal").textContent  = phase.goal;
  document.getElementById("phase-desc").textContent  = phase.description;
  const pct = ((selectedWeek - phase.weekRange[0]) / (phase.weekRange[1] - phase.weekRange[0] + 1)) * 100;
  document.getElementById("phase-progress").style.width      = pct + "%";
  document.getElementById("phase-progress").style.background = phase.accent;
  document.getElementById("phase-progress-label").textContent =
    "Week " + (selectedWeek - phase.weekRange[0] + 1) + " of " +
    (phase.weekRange[1] - phase.weekRange[0] + 1) + " in this phase";
  document.getElementById("goal-val").style.color = phase.accent;

  // Week grid
  const grid = document.getElementById("week-grid");
  grid.innerHTML = "";
  for (let w = 1; w <= 52; w++) {
    const p     = getPhase(w);
    const isSel = w === selectedWeek;
    const done  = isWeekDone(w);
    const wrap  = document.createElement("div");
    wrap.className = "week-btn-wrap";
    const btn = document.createElement("button");
    btn.className        = "week-btn";
    btn.textContent      = w;
    btn.style.background = isSel ? p.accent : p.color + "55";
    btn.style.color      = isSel ? "#fff"   : "#333";
    btn.style.border     = isSel ? "2px solid " + p.accent : "2px solid transparent";
    btn.style.fontWeight = isSel ? "700" : "400";
    btn.addEventListener("click", () => { selectedWeek = w; expandedEx = null; render(); });
    wrap.appendChild(btn);
    if (done) {
      const chk = document.createElement("div");
      chk.className   = "week-check";
      chk.textContent = "✓";
      wrap.appendChild(chk);
    }
    grid.appendChild(wrap);
  }

  // Phase key
  const key = document.getElementById("phase-key");
  key.innerHTML = "";
  PHASES.forEach(p => {
    const item = document.createElement("div");
    item.className = "phase-key-item";
    item.innerHTML =
      '<div class="phase-key-dot" style="background:' + p.accent + '"></div>' +
      '<span>' + p.name + ' (Wks ' + p.weeks + ')</span>';
    key.appendChild(item);
  });

  // Mark Done button
  const mdb = document.getElementById("mark-done-btn");
  const thisDayDone = isDayDone(selectedWeek, selectedDay);
  if (thisDayDone) {
    mdb.className   = "done";
    mdb.textContent = "✓ Day " + selectedDay + " Done  (tap to undo)";
  } else {
    mdb.className   = "not-done";
    mdb.textContent = "☐ Mark Day " + selectedDay + " Done";
  }

  // Day section label
  document.getElementById("day-section-label").textContent =
    "Week " + selectedWeek + " \u2014 Choose Your Day";

  // Day tabs
  const tabs = document.getElementById("day-tabs");
  tabs.innerHTML = "";
  ["A","B","C"].forEach(d => {
    const w    = structure[d];
    const done = isDayDone(selectedWeek, d);
    const btn  = document.createElement("button");
    btn.className         = "day-tab";
    btn.style.background  = selectedDay === d ? DAY_COLORS[d] : "transparent";
    btn.style.borderColor = selectedDay === d ? DAY_COLORS[d] : "#ddd";
    btn.style.fontWeight  = selectedDay === d ? "700" : "400";
    const badge = done ? '<span class="day-done-badge">✓</span>' : "";
    btn.innerHTML =
      '<span class="day-icon">' + DAY_ICONS[d] + '</span>' +
      '<div class="day-tab-label">' + w.label + badge + '</div>' +
      '<div class="day-tab-focus">' + w.focus + '</div>';
    btn.addEventListener("click", () => { selectedDay = d; expandedEx = null; render(); });
    tabs.appendChild(btn);
  });

  // Cardio
  document.getElementById("cardio-card").style.borderColor = phase.accent;
  document.getElementById("cardio-machine").textContent    = cardio.machine;
  document.getElementById("cardio-duration").textContent   = cardio.duration;
  document.getElementById("cardio-intensity").textContent  = cardio.intensity;
  document.getElementById("cardio-notes").textContent      = cardio.notes;

  // Exercise section label
  document.getElementById("ex-section-label").textContent =
    DAY_ICONS[selectedDay] + " " + workout.label;

  // Exercises — weights now shown as formula suggestions
  // (will be replaced by API suggestions in the logging screen)
  const baseWeights = { 1:30,2:40,3:40,4:30,5:30,6:30,7:40,8:30,
                        9:45,10:45,11:40,12:90,13:40,14:50,15:50,16:60,
                        17:50,18:50,19:35,20:30,21:100,22:30,23:40 };

  const list = document.getElementById("ex-list");
  list.innerHTML = "";
  workout.exercises.forEach((ex, i) => {
    const isOpen    = expandedEx === i;
    const name      = getExerciseName(ex.exerciseId);
    const baseW     = baseWeights[ex.exerciseId] ?? 30;
    const weight    = scaleWeight(baseW, selectedWeek);
    const isAssist  = ex.exerciseId === 21;

    const card = document.createElement("div");
    card.className = "ex-card";
    card.style.borderLeftColor = DAY_COLORS[selectedDay];

    const header = document.createElement("button");
    header.className = "ex-header";
    header.innerHTML =
      '<div class="ex-num">' + (i+1) + '</div>' +
      '<div class="ex-info">' +
        '<div class="ex-name">' + name + '</div>' +
        '<div class="ex-target">' + ex.target + '</div>' +
      '</div>' +
      '<div class="ex-badges">' +
        '<span class="badge">' + ex.sets + ' sets</span>' +
        '<span class="badge">' + ex.reps + ' reps</span>' +
        '<span class="badge badge-weight" style="background:' + phase.accent + '">' +
          weight + ' lbs' + (isAssist ? " assist" : "") +
        '</span>' +
      '</div>' +
      '<div class="ex-chevron">' + (isOpen ? '▲' : '▼') + '</div>';
    header.addEventListener("click", () => { expandedEx = expandedEx===i?null:i; render(); });

    const panel = document.createElement("div");
    panel.className = "ex-panel" + (isOpen ? "" : " hidden");

    const notesRow = document.createElement("div");
    notesRow.className = "ex-notes-row";
    notesRow.innerHTML = '<span>💡</span> ' + ex.notes;
    panel.appendChild(notesRow);

    const links = document.createElement("div");
    links.className = "ex-links";

    const imgBtn = document.createElement("a");
    imgBtn.className = "ex-link-btn btn-images";
    imgBtn.href = googleImagesUrl(name);
    imgBtn.target = "_blank"; imgBtn.rel = "noopener noreferrer";
    imgBtn.innerHTML = '<span class="btn-icon">🖼️</span> View Images';

    const ytBtn = document.createElement("a");
    ytBtn.className = "ex-link-btn btn-youtube";
    ytBtn.href = youtubeUrl(name);
    ytBtn.target = "_blank"; ytBtn.rel = "noopener noreferrer";
    ytBtn.innerHTML = '<span class="btn-icon">▶</span> Watch Tutorial';

    links.appendChild(imgBtn);
    links.appendChild(ytBtn);
    panel.appendChild(links);
    card.appendChild(header);
    card.appendChild(panel);
    list.appendChild(card);
  });
}

// ── Mark Done button handler ──────────────────────────────────────────────────

document.getElementById("mark-done-btn").addEventListener("click", async () => {
  const thisDayDone = isDayDone(selectedWeek, selectedDay);
  if (thisDayDone) {
    // Undo — unmark the day
    await unmarkDay(selectedWeek, selectedDay);
    render();
  } else {
    // Show logging screen
    await showLoggingScreen(selectedWeek, selectedDay);
  }
});

// ── Init ──────────────────────────────────────────────────────────────────────

async function init() {
  // Load exercises for all days in parallel to populate cache
  await Promise.all([
    loadExercisesForDay("A"),
    loadExercisesForDay("B"),
    loadExercisesForDay("C"),
    loadProgress()
  ]);
  render();
  document.getElementById("loading-overlay").style.display = "none";
}

init();
