# Οδηγίες Εισαγωγής CSV στο Excel για Γραφήματα

## Αρχεία CSV που Δημιουργούνται

Η εφαρμογή δημιουργεί **3 CSV αρχεία** στον φάκελο `Output/`:

### 1. **solunar_periods_*.csv** - Περίοδοι Major/Minor
Περιέχει:
- Period Type (Τύπος περιόδου)
- Start Time (Ώρα έναρξης)
- End Time (Ώρα λήξης)  
- Center Time (Κεντρική ώρα)
- Duration (Διάρκεια σε λεπτά)

### 2. **solunar_hourly_*.csv** - Ωριαία Δραστηριότητα
Περιέχει:
- Hour (Ώρα 0-23)
- Time (Ώρα σε μορφή HH:MM)
- Activity Score (Συνολικό σκορ 0-100)
- Solunar Score (Βασικό solunar σκορ)
- Weather Modifier (Τροποποιητής καιρού)
- Tide Modifier (Τροποποιητής παλίρροιας)
- Total Components (Άθροισμα συστατικών)

### 3. **solunar_summary_*.csv** - Περίληψη
Περιέχει γενικές πληροφορίες (ημερομηνία, τοποθεσία, φάση σελήνης, rating κλπ.)

---

## Εισαγωγή στο Excel

### Βήμα 1: Άνοιγμα CSV
1. Άνοιξε το Excel
2. Πήγαινε στο **Data** → **From Text/CSV** (ή **Get Data** → **From File** → **From Text/CSV**)
3. Επίλεξε το CSV αρχείο (π.χ. `solunar_hourly_*.csv`)

### Βήμα 2: Ρύθμιση Εισαγωγής
1. Στο παράθυρο εισαγωγής:
   - **File Origin**: UTF-8
   - **Delimiter**: Comma
   - **Data Type Detection**: Based on entire dataset
2. Πάτησε **Load**

### Βήμα 3: Διόρθωση Δεκαδικών (Σημαντικό!)
Επειδή τα ελληνικά Excel χρησιμοποιούν κόμμα (,) αντί για τελεία (.) στα δεκαδικά:

**Επιλογή A - Αλλαγή στο Excel:**
1. Επίλεξε τις στήλες με αριθμούς (π.χ. Solunar Score, Weather Modifier, Tide Modifier)
2. Πάτησε **Ctrl+H** (Find & Replace)
3. Find: `.` (τελεία)
4. Replace: `,` (κόμμα)
5. Replace All

**Επιλογή B - Χρήση Power Query:**
1. Πριν πατήσεις Load, πάτησε **Transform Data**
2. Επίλεξε τις στήλες με αριθμούς
3. Κάνε δεξί κλικ → **Change Type** → **Decimal Number**
4. Close & Load

---

## Δημιουργία Γραφημάτων

### Γράφημα 1: Ωριαία Δραστηριότητα (Line Chart)

**Χρησιμοποίησε**: `solunar_hourly_*.csv`

1. Επίλεξε τις στήλες **Hour** και **Activity Score**
2. Insert → Line Chart → Line with Markers
3. Προσθήκη τίτλου: "Hourly Activity Score"
4. Άξονας X: Hour (0-23)
5. Άξονας Y: Activity Score (0-100)

**Για πιο λεπτομερές γράφημα:**
- Επίλεξε **Hour**, **Solunar Score**, **Weather Modifier**, **Tide Modifier**
- Insert → Line Chart → Stacked Line
- Αυτό δείχνει τη συνεισφορά κάθε παράγοντα

### Γράφημα 2: Σύγκριση Συστατικών (Stacked Column)

**Χρησιμοποίησε**: `solunar_hourly_*.csv`

1. Επίλεξε στήλες: **Hour**, **Solunar Score**, **Weather Modifier**, **Tide Modifier**
2. Insert → Column Chart → Stacked Column
3. Τίτλος: "Activity Score Components by Hour"
4. Αυτό δείχνει πώς κάθε παράγοντας συνεισφέρει ανά ώρα

### Γράφημα 3: Χρονοδιάγραμμα Περιόδων (Timeline/Gantt)

**Χρησιμοποίησε**: `solunar_periods_*.csv`

1. Επίλεξε **Period Type**, **Start Time**, **End Time**
2. Insert → Bar Chart → Stacked Bar
3. Αυτό δείχνει πότε συμβαίνουν οι Major και Minor περίοδοι

### Γράφημα 4: Σύγκριση με Μέσο Όρο

**Χρησιμοποίησε**: `solunar_hourly_*.csv` + `solunar_summary_*.csv`

1. Από το summary, πάρε το **Average Activity Score** (π.χ. 38)
2. Στο hourly sheet, δημιούργησε νέα στήλη "Average" με σταθερή τιμή 38
3. Επίλεξε **Hour**, **Activity Score**, **Average**
4. Insert → Line Chart → Line
5. Αυτό δείχνει τις ώρες που είναι πάνω/κάτω από το μέσο όρο

---

## Προτεινόμενο Dashboard Layout

### Σελίδα 1: Ωριαία Ανάλυση
- **Γράφημα Α**: Line chart με Activity Score (0-100) ανά ώρα
- **Γράφημα Β**: Stacked column με breakdown (Solunar/Weather/Tide)
- **Πίνακας**: Top 5 ώρες με υψηλότερο σκορ

### Σελίδα 2: Περίοδοι
- **Γράφημα Γ**: Timeline με Major/Minor periods
- **Πίνακας**: Λίστα περιόδων με ώρες

### Σελίδα 3: Περίληψη
- **KPIs**: Moon Phase, Rating, Average Score
- **Στατιστικά**: Min/Max/Average scores
- **Peak Hours**: Highlight των καλύτερων ωρών

---

## Παράδειγμα Formulas για Ανάλυση

### Εύρεση Peak Hours (σκορ >= 80):
```excel
=IF(C2>=80, A2, "")
```

### Υπολογισμός Διαφοράς από Μέσο Όρο:
```excel
=C2-AVERAGE($C$2:$C$25)
```

### Κατάταξη Ωρών:
```excel
=RANK(C2, $C$2:$C$25, 0)
```

### Conditional Formatting για Θερμοχρωματισμό:
1. Επίλεξε τη στήλη **Activity Score**
2. Home → Conditional Formatting → Color Scales
3. Επίλεξε Red-Yellow-Green scale

---

## Tips για καλύτερα Γραφήματα

1. **Χρησιμοποίησε Data Labels** για τις peak ώρες
2. **Προσθήκη Horizontal Line** στο μέσο όρο (38) για reference
3. **Διαφορετικά χρώματα** για Major/Minor periods
4. **Annotations** στις ώρες ανατολής/δύσης ηλίου
5. **Zoom in** σε συγκεκριμένες ώρες ενδιαφέροντος

---

## Αυτοματοποίηση με Macro (Advanced)

Αν θέλεις να αυτοματοποιήσεις τη διαδικασία:

1. Καταγραφή Macro κατά την εισαγωγή του πρώτου CSV
2. Αποθήκευση ως Template
3. Κάθε φορά που τρέχεις τον υπολογισμό, ανανέωση δεδομένων με **Data → Refresh All**

---

## Troubleshooting

**Πρόβλημα**: Τα δεκαδικά εμφανίζονται λάθος
**Λύση**: Χρήση Find & Replace (. → ,) ή αλλαγή Regional Settings

**Πρόβλημα**: Οι ώρες δεν αναγνωρίζονται σωστά
**Λύση**: Format Cells → Custom → "HH:MM"

**Πρόβλημα**: Το γράφημα δεν δείχνει 24 ώρες
**Λύση**: Βεβαιώσου ότι η στήλη Hour είναι 0-23 (όχι 1-24)

---

**Καλή Ανάλυση!** 🎣📊
