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
Customer goes through different mood states at set intervals until they leave or get served. - Not in Tutorial.

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

### TutorialManager ([source](Assets/Scripts/Manager/TutorialManager.cs))
Directs tutorial phases: frying eggplant, assembling sandwiches, and serving customers. Controls arrows, item clickability, customer rounds, and `GamePhase` updates.
Key functions:
- `OnIngredientClicked(Item,string)` ([link](Assets/Scripts/Manager/TutorialManager.cs#L65-L86)) – Routes clicks by tutorial phase, starting frying or advancing sandwich steps.
- `OnEggplantTrayFull()` ([link](Assets/Scripts/Manager/TutorialManager.cs#L117-L135)) – Switches to assembly, disables the eggplant row, enables the next ingredient, and updates arrows and phase.

---

### GamePhase ([source](Assets/Scripts/Manager/GamePhase.cs))
Enum describing gameplay phases across the tutorial loop. Used by `GameFlowManager` to drive instruction text.
Key values: `AddRowEggplant`, `FryingEggplant`, `AssembleDish`, `ServeCustomer`, `NextCustomer`.

---

## Player

### Item ([source](Assets/Scripts/Player/ItemScript.cs))
Clickable ingredient that can join the current selection. Delegates tutorial progression after a successful add.
Key functions:
- `OnClick()` ([link](Assets/Scripts/Player/ItemScript.cs#L8-L43)) – Adds the ingredient via `SelectionList` (unless excluded types) and notifies `TutorialManager`.

---

### FryZoneEggplant ([source](Assets/Scripts/Player/FryZoneEggplant.cs))
State machine for the frying pan, handling timing, visuals, and readiness. Hands fried eggplant to the tray on click when ready.
Key functions:
- `Update()` ([link](Assets/Scripts/Player/FryZoneEggplant.cs#L25-L44)) – Advances the fry timer while frying and transitions to `Ready` when time elapses.
- `OnMouseDown()` ([link](Assets/Scripts/Player/FryZoneEggplant.cs#L94-L107)) – When ready, clears the pan and calls `EggplantTray.FillFromPan()` to deliver eggplant.

---

### EggplantTray ([source](Assets/Scripts/Player/EggplantTray.cs))
Tracks tray fill visuals and signals tutorial progress when full. Starts empty and swaps to a full sprite once filled from the pan.
Key functions:
- `FillFromPan()` ([link](Assets/Scripts/Player/EggplantTray.cs#L27-L43)) – Enables the full-tray sprite and calls `TutorialManager.OnEggplantTrayFull()`.

---

## Utilities

### UIInstructions ([source](Assets/Scripts/Util/UIInstructions.cs))
Wrapper over TMP text for on-screen prompts. Caches the latest instruction string for reads.
Key functions:
- `SetInstructions(string)` ([link](Assets/Scripts/Util/UIInstructions.cs#L12-L20)) – Clears and writes the provided message to the TMP component.
- `GetCurrentText()` ([link](Assets/Scripts/Util/UIInstructions.cs#L22-L25)) – Returns the last stored instruction string.

---

### FryTimerUI ([source](Assets/Scripts/Util/FryTimerUI.cs))
Displays frying progress as a UI fill tied to the pan state. Hides itself when no frying is active.
Key functions:
- `Update()` ([link](Assets/Scripts/Util/FryTimerUI.cs#L23-L41)) – Toggles visibility by `IsFrying` and sets `fillAmount` to `1 - FryProgress` while active.

---

### MuteButton ([source](Assets/Scripts/Util/MuteButton.cs))
UI toggle bound to the global music mute state. Swaps sprites to reflect mute/unmute after presses.
Key functions:
- `OnPress()` ([link](Assets/Scripts/Util/MuteButton.cs#L15-L21)) – Calls `MusicPlayer.ToggleSound()` and refreshes the icon.
- `UpdateIcon()` ([link](Assets/Scripts/Util/MuteButton.cs#L23-L36)) – Selects mute or unmute sprite based on `MusicPlayer` state.

---


<a href="https://itamar-raz-dev-game.itch.io/sababich-tutorial">link</a>





