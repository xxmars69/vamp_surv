# Joc 2D Survival - Bullet Heaven (Stadiul Curent)

Acesta este un proiect de joc 2D în Unity, inspirat din mecanicile jocurilor de tip "Bullet Heaven" (ex: Vampire Survivors). Jucătorul trebuie să supraviețuiască unor valuri de inamici, să colecteze resurse și să își îmbunătățească abilitățile pe parcurs.

## Starea Actuală a Proiectului

Proiectul conține mecanicile de bază complet funcționale, o structură clară pentru statistici și upgrade-uri, un sistem de inamici și o interfață grafică (UI) funcțională.

### 1. Sistemul Jucătorului (Player)
*   **Mişcare:** Jucătorul se deplasează pe axele X și Y folosind tastele de direcție (sau WASD). Viteza este influențată de statistica `MoveSpeed`. Sprite-ul se întoarce automat (flip) în funcție de poziția mouse-ului.
*   **Sănătate (Health):** Jucătorul are o viață de bază (implicit 6 inimi). Ia damage la coliziunea cu inamicii (damage-ul luat este redus de statistica `Armor`). Când este lovit, sprite-ul clipește roșu.
*   **Atac Automat:** Jucătorul trage automat (Auto-Attack) cu proiectile în direcția cursorului de la mouse. Frecvența, numărul de proiectile și mărimea lor sunt afectate de statisticile jucătorului (`Cooldown`, `Amount`, `Area`). Există și un upgrade de tip **Scut Orbital**.

### 2. Sistemul de Inamici
*   **Spawn:** `EnemySpawner` generează inamici într-o rază în jurul jucătorului la intervale regulate de timp.
*   **Comportament:** Inamicii se deplasează constant spre jucător.
*   **Recompense (Drops):** Când mor, inamicii cresc contorul total de kill-uri și lasă în urmă pietre de experiență (XP Gems). Pietrele albastre cad la fiecare kill, iar cele roșii (probabil de valoare mai mare) la fiecare al doilea kill. Există și un sistem implementat separat pentru monede/aur (Gold/Platinum Coins, Money Bags).

### 3. Sistemul de Progresie și Upgrade-uri (VampireSystem)
Acesta este nucleul jocului, bazat pe o arhitectură robustă cu `ScriptableObjects`.
*   **Statistici Complexe:** Jocul urmărește numeroase statistici: `Might`, `Armor`, `MaxHealth`, `Recovery`, `Cooldown`, `Area`, `Speed`, `Duration`, `Amount`, `MoveSpeed`, `Magnet`, `Luck`, `Growth`, `Greed`, `Curse`, `Charm`.
*   **Stacking (Cumulare):** Statisticile pot fi adunate liniar (Additive) sau înmulțite (Multiplicative).
*   **Experiență (XP):** Jucătorul colectează `XPGem`-uri (care îl urmăresc dacă e aproape). Colectarea crește bara de XP (influențat de statistica `Growth`).
*   **Level Up:** Când se atinge pragul de XP (care crește cu 25% la fiecare nivel), jocul pune pauză (Time.timeScale = 0) și afișează un meniu de upgrade.
*   **Item Picker:** Meniul de upgrade alege aleatoriu 3 obiecte dintr-o listă de iteme disponibile, ținând cont de raritatea lor (sistem de selecție ponderată/weighted random).

### 4. Interfața Grafică (UI)
*   **HUD în Joc:** Afișează Nivelul (LVL), Experiența (XP), Aurul (Gold) și Viețile (Inimioare) în timp real.
*   **Meniul Principal:** Conține funcționalitatea de bază: buton de Play (care încarcă `SampleScene`), un meniu de selecție a personajului (placeholder) și un panou cu Credite ("Marius Mihai si Andrei").

### 5. Reparații la Rulare (Runtime Repair)
*   Codul conține o clasă ingenioasă `RuntimeVisualRepair` care se asigură că la rularea jocului, UI-ul de viață, sprite-ul jucătorului și fundalul infinit (din iarbă) sunt instantiate și conectate corect, chiar dacă lipsesc din scena editorului.

## Ce Nu Mai Este în Proiect
Au fost curățate recent scripturile de Editor (`SetupBackground.cs`, `SetupEnemySprites.cs`, `ShowSpriteIndices.cs`, etc.) care creau un meniu "Joc 2D" în bara de sus a Unity-ului, pentru a păstra proiectul mai curat.

---
**Tehnologii și Arhitectură:**
*   Proiectul este dezvoltat în **Unity** (folosind C#).
*   Folosește intensiv **ScriptableObjects** pentru definirea itemelor și a statisticilor jucătorului, ceea ce face scalarea jocului (adăugarea de noi iteme/arme) foarte ușoară.
*   Render mode-ul folosit este orientat spre 2D (SpriteRenderers, Rigidbody2D).
