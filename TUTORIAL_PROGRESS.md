# Progres față de tutorialul Terresquall (Vampire Survivors clone)

Serie completă: https://blog.terresquall.com/series/creating-a-rogue-like-vampire-survivors/

> **Notă importantă:** noi NU am urmat tutorialul pas-cu-pas (doar Partea 2 - harta).
> Am construit o versiune proprie care acoperă majoritatea funcționalităților, dar cu
> arhitectură diferită. Acest document arată unde suntem față de fiecare parte și ce
> mai trebuie pentru a fi "exact ca în tutorial".

Legendă: ✅ Făcut · ⚠️ Parțial · ❌ Lipsește

---

## Tabel comparativ (28 părți)

| # | Parte | Link | Status la noi | Ce avem / ce lipsește |
|---|-------|------|---------------|------------------------|
| 1 | Movement and Camera | https://blog.terresquall.com/2022/11/creating-a-rogue-like-vampire-survivors-part-1/ | ✅ | `PlayerController.cs` (WASD), `CameraFollow.cs` (pixel-snap) |
| 2 | Map Generation | https://blog.terresquall.com/2022/12/creating-a-rogue-like-vampire-survivors-part-2/ | ✅ | Urmat EXACT: `MapController`, `ChunkTrigger`, `PropRandomizer` |
| 3 | Weapons and Enemy AI | https://blog.terresquall.com/2022/12/creating-a-rogue-like-vampire-survivors-part-3/ | ✅ | `AutoAttack.cs`, `Enemy.cs` (urmărește jucătorul) |
| 4 | Weapon and Enemy Stats | https://blog.terresquall.com/2022/12/creating-a-rogue-like-vampire-survivors-part-4/ | ⚠️ | Avem `PlayerStatsSO`, Enemy are HP/speed. Lipsește un sistem structurat de WeaponStats |
| 5 | Characters and Pick-ups | https://blog.terresquall.com/2023/01/creating-a-rogue-like-vampire-survivors-part-5/ | ✅ | 5 personaje, `CharacterProfile`, pickups (XP, gold, Ham) |
| 6 | Character Add-ons | https://blog.terresquall.com/2023/02/creating-a-rogue-like-vampire-survivors-part-6/ | ⚠️ | Avem stats per personaj. Lipsește arma de start specifică per personaj |
| 7 | Enemy Spawning | https://blog.terresquall.com/2023/02/creating-a-rogue-like-vampire-survivors-part-7/ | ✅ | `EnemySpawner` + `EnemyWaveManager` (valuri, pe minute, cap 20) |
| 8 | Passive Items and Inventory | https://blog.terresquall.com/2023/03/creating-a-rogue-like-vampire-survivors-part-8/ | ⚠️ | Avem iteme pasive. Lipsește UI-ul de inventar |
| 9 | Game Manager and UI 1 | https://blog.terresquall.com/2023/04/creating-a-rogue-like-vampire-survivors-part-9/ | ✅ | `GameManager.cs` + HUD |
| 10 | Game Manager and UI 2 | https://blog.terresquall.com/2023/05/creating-a-rogue-like-vampire-survivors-part-10/ | ✅ | Pauză, game over cu rezultate, level-up UI |
| 11 | Code and Aesthetic Touch-ups | https://blog.terresquall.com/2023/07/creating-a-rogue-like-vampire-survivors-part-11/ | ⚠️ | Polissh continuu |
| 12 | **Weapon Evolution** | https://blog.terresquall.com/2023/07/creating-a-rogue-like-vampire-survivors-part-12/ | ❌ | Lipsește: combini armă+pasiv la nivel max → armă evoluată |
| 13 | **Knockback and Damage Feedback** | https://blog.terresquall.com/2023/09/creating-a-rogue-like-vampire-survivors-part-13/ | ⚠️ | Avem flash + damage numbers. Lipsește knockback (inamicii împinși la lovire) |
| 14 | Damage Floating Text | https://blog.terresquall.com/2023/10/creating-a-rogue-like-vampire-survivors-part-14/ | ✅ | `DamageNumber.cs` |
| 15 | Weapon System Revamp | https://blog.terresquall.com/2023/12/creating-a-rogue-like-vampire-survivors-part-15/ | ❌ | Tutorial: clasă `Weapon` modulară. Noi: `AutoAttack` monolitic |
| 15.5 | Get Unique Character Sprites | https://blog.terresquall.com/2024/07/creating-a-rogue-like-vampire-survivors-in-unity-part-15-5/ | ✅ | Sprite-uri unice pentru toate personajele |
| 16 | New Weapons and Bugfixes | https://blog.terresquall.com/2024/01/creating-a-rogue-like-vampire-survivors-part-16/ | ⚠️ | Avem multe arme (gloanțe, coasă, cărți, blood charge) |
| 17 | Fixing Pickup Collection | https://blog.terresquall.com/2024/02/creating-a-rogue-like-vampire-survivors-part-17/ | ✅ | Pickups funcționează + magnet |
| 18 | Dynamic Stats System and UI | https://blog.terresquall.com/2024/03/creating-a-rogue-like-vampire-survivors-part-18/ | ⚠️ | Avem `PlayerStatsSO` + panou stats |
| 19 | **Upgrading the Level-up UI** | https://blog.terresquall.com/2024/04/creating-a-rogue-like-vampire-survivors-part-19/ | ⚠️ | Avem level-up UI basic. Lipsesc culori de raritate + iconițe mai mari |
| 20 | **Upgrading the Inventory UI** | https://blog.terresquall.com/2024/05/creating-a-rogue-like-vampire-survivors-part-20/ | ❌ | Lipsește UI de inventar (ce iteme ai luat) |
| 21 | Upgrading the Enemy Spawn System | https://blog.terresquall.com/2024/07/creating-a-rogue-like-vampire-survivors-part-21/ | ⚠️ | Avem spawn pe minute |
| 22 | **Curse Boosts and All Enemy Stats** | https://blog.terresquall.com/2024/07/creating-a-rogue-like-vampire-survivors-part-22/ | ❌ | Stat-ul Curse există în enum dar nu scalează inamicii |
| 23 | **Buff / Debuff System** | https://blog.terresquall.com/2024/08/creating-a-rogue-like-vampire-survivors-part-23/ | ❌ | Lipsește |
| 24 | Next-Level Character Select | https://blog.terresquall.com/2025/01/creating-a-rogue-like-vampire-survivors-part-24/ | ✅ | Avem ecran de selecție frumos cu panou stats |
| 25 | **Level Select Screen** | https://blog.terresquall.com/2025/03/creating-a-rogue-like-vampire-survivors-part-25/ | ❌ | Avem un singur nivel (SampleScene) |
| 26 | **Coins and Save System** | https://blog.terresquall.com/2025/05/creating-a-rogue-like-vampire-survivors-part-26/ | ⚠️ | Avem gold. Lipsește salvarea (persistență) |
| 27 | Treasure Chest UI Animation | https://blog.terresquall.com/2025/08/creating-a-rogue-like-vampire-survivors-part-27/ | ⚠️ | Avem cufăr (de la miniboss). Lipsește animația de deschidere |
| 28 | **Persistent Power Ups** | https://blog.terresquall.com/2026/03/creating-a-rogue-like-vampire-survivors-part-28/ | ❌ | Lipsește meta-progresia (upgrade-uri permanente cu gold) |

---

## Concluzie: unde suntem

- **Gameplay de bază (Părțile 1-11): ~90% complet.** Avem mișcare, hartă, arme, inamici, personaje,
  pickups, spawn, game manager, UI. Funcțional, dar cu cod diferit de tutorial.
- **Feature-uri avansate (Părțile 12-28): ~40% complet.** Avem damage text, character select, cufăr basic.
  Lipsesc: evoluții de arme, knockback, inventar UI, level select, save system, meta-progresie.

**Ca să fie "exact ca în tutorial"** lipsesc în principal aceste 8 lucruri (în ordinea priorității):

---

## PLAN pe pași (ce mai trebuie adăugat)

### 🔴 Prioritate mare (impact mare, definesc un VS clone)

#### Pas 1 — Knockback la inamici (Partea 13)
- 📖 https://blog.terresquall.com/2023/09/creating-a-rogue-like-vampire-survivors-part-13/
- **Ce face:** când lovești un inamic, e împins ușor înapoi (feedback satisfăcător).
- **La noi:** în `Enemy.TakeDamage()` adăugăm un mic impuls în direcția opusă jucătorului.
- **Efort:** mic (1 fișier).

#### Pas 2 — Evoluții de arme (Partea 12)
- 📖 https://blog.terresquall.com/2023/07/creating-a-rogue-like-vampire-survivors-part-12/
- **Ce face:** o armă la nivel max + un pasiv specific la nivel max → se transformă în armă evoluată mai puternică.
- **Exemplu la noi:** Revolver (nivel 4) + Dagger (nivel 5) → "Revolver Auriu" cu damage dublu.
- **Efort:** mediu (logică în `GameManager` + 1-2 iteme noi).

#### Pas 3 — Inventar UI (Partea 8 + 20)
- 📖 https://blog.terresquall.com/2023/03/creating-a-rogue-like-vampire-survivors-part-8/
- 📖 https://blog.terresquall.com/2024/05/creating-a-rogue-like-vampire-survivors-part-20/
- **Ce face:** o bară cu iconițele itemelor pe care le-ai luat (jos sau în colț), cu nivelul fiecăruia.
- **La noi:** citim din `Player.Instance.PlayerStats.LevelObject` și afișăm iconițele.
- **Efort:** mediu.

### 🟡 Prioritate medie (adâncime, conținut)

#### Pas 4 — Level-up UI cu raritate (Partea 19)
- 📖 https://blog.terresquall.com/2024/04/creating-a-rogue-like-vampire-survivors-part-19/
- **Ce face:** cardurile de upgrade au culoare de raritate (alb/albastru/auriu) + iconițe mari + descriere.
- **Efort:** mic-mediu (doar UI în `GameManager.CreateItemButton`).

#### Pas 5 — Curse + scalarea inamicilor pe timp (Partea 22)
- 📖 https://blog.terresquall.com/2024/07/creating-a-rogue-like-vampire-survivors-part-22/
- **Ce face:** inamicii devin mai puternici (HP/speed/damage) pe măsură ce trece timpul; stat-ul Curse accelerează asta.
- **La noi:** scalăm `Enemy.maxHealth/speed` după minut în spawner.
- **Efort:** mic.

#### Pas 6 — Buff / Debuff System (Partea 23)
- 📖 https://blog.terresquall.com/2024/08/creating-a-rogue-like-vampire-survivors-part-23/
- **Ce face:** efecte temporare (ex. încetinire inamici, boost de viteză la pickup).
- **Efort:** mediu.

### 🟢 Prioritate mică (meta-joc, opțional)

#### Pas 7 — Level Select Screen (Partea 25)
- 📖 https://blog.terresquall.com/2025/03/creating-a-rogue-like-vampire-survivors-part-25/
- **Ce face:** ecran de alegere a hărții/stage-ului înainte de joc.
- **Efort:** mediu (scenă nouă + UI).

#### Pas 8 — Save System + Persistent Power Ups (Părțile 26 + 28)
- 📖 https://blog.terresquall.com/2025/05/creating-a-rogue-like-vampire-survivors-part-26/
- 📖 https://blog.terresquall.com/2026/03/creating-a-rogue-like-vampire-survivors-part-28/
- **Ce face:** gold-ul se salvează între runde (`PlayerPrefs`); îl cheltui pe upgrade-uri permanente
  (ex. +10% damage la toate rundele).
- **Efort:** mediu-mare.

#### Pas 9 — Animație cufăr (Partea 27)
- 📖 https://blog.terresquall.com/2025/08/creating-a-rogue-like-vampire-survivors-part-27/
- **Ce face:** cufărul se deschide cu animație + roată de reward (slot machine).
- **Efort:** mediu.

---

## Recomandare ordine de implementare

1. **Knockback** (Pas 1) — instant satisfăcător, efort minim
2. **Curse / scalare inamici** (Pas 5) — face jocul progresiv mai greu
3. **Inventar UI** (Pas 3) — vezi ce ai luat
4. **Level-up UI cu raritate** (Pas 4) — arată mult mai profesionist
5. **Evoluții de arme** (Pas 2) — adâncime de build
6. Restul (buff/debuff, level select, save, animație cufăr) — după gust

> Resursa principală pentru orice pas: pagina tutorialului din link. Codul lor diferă de al
> nostru (ei folosesc clase `Weapon`/`PassiveItem`/`PlayerStats` modulare), deci adaptăm ideea,
> nu copiem direct.
