# Distorted Presence

**Distorted Presence** je atmosférická hororová hra z pohledu první osoby (First-Person Anomaly Hunting Game) vytvořená v enginu Unity. Hráč se ocitá v temném, měnícím se prostředí (hrad, kobky, historické síně), kde je jeho jediným úkolem přežít 5 dní tím, že bude dokumentovat a odhalovat paranormální anomálie pomocí fotoaparátu, zatímco si musí udržet své duševní zdraví (sanity).

---

## 📖 Příběh & Atmosféra

> *"Zůstalo jen ticho. Staré zdi pohlcují zvuky minulosti. Říká se, že realita zde neudržela svou podobu – praskla, zkroutila se a rozpadla. Někdy jsou změny viditelné... jindy ne. Ty, kdo čteš tyto řádky, jsi další, kdo může sledovat zbytky světa, který se ti mění před očima.*
>
> *Pamatuj – čím víc toho přehlédneš, tím méně z tebe zbude."*

---

## 🎮 Průběh Hry & Smyčka (Gameplay Loop)

Hra se odehrává v průběhu **5 po sobě jdoucích dní**. Každý den představuje intenzivní směnu plnou napětí:

1. **Příprava na začátek dne**:
   - **Den 1**: Hráč začíná v uzamčené místnosti. Pro otevření dveří si musí nejprve přečíst knihu s instrukcemi. Po odemčení dveří musí hráč projít všechny místnosti hradu, aby se seznámil s jejich původním stavem. Teprve po navštívení všech místností začne běžet herní čas a začnou se generovat anomálie.
   - **Dny 2 až 5**: Den začíná s 20sekundovým bezpečnostním limitem (zpožděním), během kterého se hráč může rozmístit a připravit.
2. **Herní čas**:
   - Každý den trvá **120 sekund** reálného času, během nichž na hodinách ubíhá čas od **00:00 do 06:00 AM**.
   - Během noci se v náhodných intervalech objevují různé typy anomálií. Obtížnost stoupá s každým dnem – anomálie se generují častěji a penalizace za jejich přehlédnutí je vyšší.
3. **Ukončení dne**:
   - Den končí buď **úspěšným přežitím do 06:00 AM**, nebo **ručním ukončením** – hráč může přijít k únikovým dveřím a podržením klávesy **[E]** den ukončit předčasně.
   - Pro postup do dalšího dne musí hráč úspěšně vyfotografovat alespoň **70 %** všech vygenerovaných anomálií za daný den.
   - Pokud hráč podmínku nesplní, nebo pokud jeho sanita klesne na 0 %, čeká ho děsivý jumpscare a musí den opakovat s resetovaným postupem.

---

## 👻 Typy Anomálií

Anomálie se dělí do několika kategorií a každá z nich mění prostředí jiným způsobem:

* **🔴 Změna barvy (Color Change)**: Běžné objekty nebo textury v místnosti začnou pomalu měnit barvu na nepřirozenou (např. krvavě červenou).
* **📐 Změna velikosti (Scale Change)**: Některé objekty v prostředí se začnou pomalu zvětšovat nebo zmenšovat.
* **💡 Změna světla (Light Color Change)**: Lustry, svícny či jiná světla změní barvu svého svitu (např. na červenou).
* **❌ Zmizení objektu (Missing Object)**: Objekt, který v místnosti normálně stojí (židle, stůl, rekvizita), náhle zcela zmizí.
* **➕ Přidaný objekt (Added Object)**: V místnosti se objeví nový, cizí předmět, který tam původně nebyl.
* **👤 Stínová postava (Shadow Change)**: V temnotě se zhmotní děsivá stínová postava.
* **👁️ Periferní iluze (Visual Illusion)**: *Nejzákeřnější anomálie.* Objekt se chvěje a rotuje pouze v případě, že ho vidíte v periferním vidění (na okraji obrazovky). Jakmile se na něj otočíte a podíváte se na něj přímo, okamžitě se vrátí do původního nehybného stavu. Musíte ho tedy odhalit koutkem oka a rychle zaměřit.

---

## 🧠 Klíčové Herní Systémy

### 📸 Fotoaparát & Focení
* Hráč nosí fotoaparát s **omezeným počtem snímků** (základní kapacita je 10 filmů). Film je nutné šetřit!
* Stisknutím a podržením **Pravého tlačítka myši (RMB)** přiložíte fotoaparát k očím (zoom a zobrazení hledáčku UI).
* Stisknutím **Levého tlačítka myši (LMB)** během míření pořídíte fotografii. To vyvolá silný blesk osvětlující okolí.
* Pokud blesk fotoaparátu zachytí aktivní anomálii, je nahlášena (deaktivována) a hráč získává bonus k příčetnosti.
* Vyfocení místa bez anomálie spotřebuje cenný film a hráče to stojí velkou penalizaci k duševnímu zdraví.

### 🧪 Duševní zdraví (Sanity)
* Hráč začíná se **100%** duševním zdravím.
* Každá aktivní a nenahlášená anomálie pomalu odčerpává sanitu každou sekundu. Čím více anomálií ignorujete, tím rychleji šílenství postupuje.
* **Správná fotka**: Obnoví **+5 %** sanity a odstraní anomálii.
* **Chybná fotka**: Penalizuje hráče o **-10 %** sanity.
* Pokud sanita klesne na **0 %**, hráč okamžitě podlehne šílenství, je jumpscared a hra končí prohrou.

### 🏆 Vyhodnocení & Závěrečné Ranky
Po přežití všech 5 dní je hráč přenesen na obrazovku **EndingScene**, která zobrazuje statistiky celého průchodu:
* Celkový počet zdokumentovaných anomálií ku celkovému počtu vygenerovaných.
* Přesnost focení (Accuracy %).
* Interaktivní fotogalerie ze snímků, které hráč během hraní pořídil.
* **Závěrečné hodnocení (Rank)** na základě přesnosti:
  * **Rank S** – 100% přesnost
  * **Rank A** – 90 % a více
  * **Rank B** – 75 % až 89 %
  * **Rank C** – 60 % až 74 %
  * **Rank D** – Méně než 60 % přesnost

---

## ⌨️ Ovládání (Controls)

| Klávesa / Tlačítko | Akce |
| :--- | :--- |
| **W, A, S, D** | Pohyb hráče |
| **Pohyb myši** | Rozhlížení |
| **Levý Shift** (přidržení) | Sprint |
| **Mezerník (Space)** | Výskok |
| **E** | Interakce (otevření/zavření dveří, čtení knihy) |
| **E** (podržení u únikových dveří) | Předčasné ukončení dne |
| **Pravé tlačítko myši (RMB)** | Zaměření fotoaparátu (hledáček + zoom) |
| **Leve tlačítko myši (LMB)** | Pořízení snímku *(funkční pouze při míření)* |

---

## 🛠️ Technické Detaily Projektu

* **Herní Engine**: Unity 2022+ (kompatibilní s moderními verzemi).
* **Renderovací Pipeline**: Universal Render Pipeline (URP).
* **Vstupní systém**: New Input System (využívá `InputSystem_Actions`).
* **Kamerový Stack**: Hra využívá pokročilé překrývání kamer (Camera Stacking) v URP. 3D model fotoaparátu v rukou hráče je renderován na speciální overlay kameře (`ViewModelCamera`). Tím je zamezeno nepříjemnému prolínání (clippingu) fotoaparátu skrz zdi a překážky v prostředí. Podobně je řešena i overlay kamera pro jumpscary.