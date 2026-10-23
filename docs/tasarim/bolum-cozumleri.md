# Bölüm çözümleri (test için)
Her bölümün doğru çözümü; `bolum-NN.json` içindeki çözümle aynıdır (oluşturuldu: 2026-10-22).

## Bölüm 1
```python
move(East)
move(East)
move(East)
move(North)
```

## Bölüm 2
```python
move(East)
move(East)
collect()
move(North)
move(East)
collect()
```

## Bölüm 3
```python
for i in range(5):
    move(East)
    collect()
```

## Bölüm 4
```python
for i in range(4):
    move(East)
    collect()
for i in range(3):
    move(South)
    collect()
```

## Bölüm 5
```python
for i in range(2):
    move(East)
    collect()
move(North)
for i in range(3):
    move(East)
    collect()
move(North)
```

## Bölüm 6
```python
for i in range(2):
    move(East)
    move(East)
    collect()
for i in range(2):
    move(North)
    move(North)
    collect()
```

## Bölüm 7
```python
for i in range(5):
    move(East)
    collect()
move(North)
collect()
for i in range(5):
    move(West)
    collect()
```

## Bölüm 8
```python
for i in range(3):
    move(North)
    collect()
for i in range(3):
    move(East)
    collect()
for i in range(3):
    move(South)
    collect()
for i in range(2):
    move(West)
    collect()
```

## Bölüm 9
```python
for i in range(4):
    move(East)
    collect()
```

## Bölüm 10
```python
for i in range(3):
    move(North)
    collect()
for i in range(2):
    move(East)
    move(East)
    collect()
move(North)
move(North)
```

## Bölüm 11
```python
for i in range(5):
    move(North)
    move(East)
    collect()
```

## Bölüm 12
```python
move(East)
if ice_here():
    collect()
move(North)
move(East)
if ice_here():
    collect()
```

## Bölüm 13
```python
move(East)
if ice_here():
    collect()
move(East)
if ice_here():
    collect()
move(East)
if ice_here():
    collect()
```

## Bölüm 14
```python
for i in range(4):
    move(East)
if ice_here():
    collect()
move(East)
```

## Bölüm 15
```python
for i in range(5):
    move(North)
    if ice_here():
        collect()
```

## Bölüm 16
```python
if rock_ahead(East):
    move(North)
else:
    move(East)
collect()
move(East)
move(East)
collect()
```

## Bölüm 17
```python
if rock_ahead(East):
    move(North)
else:
    move(East)
collect()
move(East)
if rock_ahead(East):
    move(North)
else:
    move(East)
collect()
```

## Bölüm 18
```python
for i in range(5):
    if rock_ahead(North):
        move(East)
    else:
        move(North)
    collect()
```

## Bölüm 19
```python
for i in range(5):
    if rock_ahead(North):
        move(East)
    else:
        move(North)
        if ice_here():
            collect()
```

## Bölüm 20
```python
for i in range(7):
    if rock_ahead(North):
        move(East)
    else:
        move(North)
        if ice_here():
            collect()
```

## Bölüm 21
```python
for i in range(5):
    move(East)
    if panel_power() < 50:
        repair()
```

## Bölüm 22
```python
for i in range(5):
    move(North)
    if panel_power() == 0:
        collect()
```

## Bölüm 23
```python
for i in range(5):
    move(East)
    if panel_power() > 0:
        repair()
    else:
        collect()
```

## Bölüm 24
```python
for i in range(5):
    move(East)
    if panel_power() == 0:
        collect()
    elif panel_power() < 50:
        repair()
```

## Bölüm 25
```python
for i in range(6):
    if i < 3:
        move(East)
    else:
        move(North)
    collect()
```

## Bölüm 26
```python
for i in range(9):
    if i < 4:
        move(East)
    elif i == 4:
        move(North)
    else:
        move(West)
    collect()
```

## Bölüm 27
```python
for i in range(6):
    if i < 3:
        move(East)
    else:
        move(North)
    if panel_power() == 0:
        collect()
    elif panel_power() < 50:
        repair()
```

## Bölüm 28
```python
for i in range(6):
    if rock_ahead(North):
        move(East)
    else:
        move(North)
    if panel_power() == 0:
        collect()
    elif panel_power() < 50:
        repair()
```

## Bölüm 29
```python
for i in range(9):
    if i > 4:
        move(West)
    elif i == 4:
        move(North)
    else:
        move(East)
    if panel_power() == 0:
        collect()
    elif panel_power() < 50:
        repair()
```

## Bölüm 30
```python
for i in range(7):
    if rock_ahead(North):
        move(East)
    else:
        move(North)
    if panel_power() == 0:
        collect()
    elif panel_power() < 50:
        repair()
```

## Bölüm 31
```python
move(East)
while dust_here():
    wait()
for i in range(4):
    move(East)
```

## Bölüm 32
```python
while not rock_ahead(East):
    move(East)
```

## Bölüm 33
```python
while not rock_ahead(North):
    move(North)
while not rock_ahead(East):
    move(East)
```

## Bölüm 34
```python
for i in range(5):
    move(East)
    while dust_here():
        wait()
```

## Bölüm 35
```python
while not rock_ahead(East):
    move(East)
    while dust_here():
        wait()
```

## Bölüm 36
```python
while not rock_ahead(East):
    move(East)
    while dust_here():
        wait()
while not rock_ahead(North):
    move(North)
    while dust_here():
        wait()
```

## Bölüm 37
```python
while not rock_ahead(North):
    move(North)
    while dust_here():
        wait()
    move(East)
```

## Bölüm 38
```python
while not rock_ahead(East):
    move(East)
    while dust_here():
        wait()
    collect()
```

## Bölüm 39
```python
while not rock_ahead(East):
    move(East)
    collect()
    move(North)
    while dust_here():
        wait()
    collect()
```

## Bölüm 40
```python
while not rock_ahead(North):
    move(North)
    while dust_here():
        wait()
    collect()
while not rock_ahead(East):
    move(East)
    while dust_here():
        wait()
    collect()
```

## Bölüm 41
```python
sayac = 0
for i in range(4):
    move(East)
    collect()
    sayac += 1
report(sayac)
```

## Bölüm 42
```python
toplam = 0
for i in range(3):
    move(East)
    toplam += cable_length()
    collect()
for i in range(2):
    move(North)
    toplam += cable_length()
    collect()
report(toplam)
```

## Bölüm 43
```python
bekleme = 0
for i in range(2):
    move(East)
    while dust_here():
        wait()
        bekleme += 1
    move(East)
move(East)
report(bekleme)
```

## Bölüm 44
```python
adim = 0
while not rock_ahead(East):
    move(East)
    adim += 1
collect()
move(South)
for i in range(adim):
    move(West)
```

## Bölüm 45
```python
sayac = 0
while not rock_ahead(East):
    move(East)
    if cable_length() >= 50:
        sayac += 1
    collect()
report(sayac)
```

## Bölüm 46
```python
kalan = 300
for i in range(2):
    move(East)
kalan -= cable_length()
collect()
for i in range(2):
    move(North)
kalan -= cable_length()
collect()
for i in range(2):
    move(East)
kalan -= cable_length()
collect()
report(kalan)
```

## Bölüm 47
```python
kalan = 3
while kalan > 0:
    move(North)
    if cable_length() > 0:
        collect()
        kalan -= 1
move(East)
```

## Bölüm 48
```python
en_uzun = 0
while not rock_ahead(East):
    move(East)
    u = cable_length()
    if u > en_uzun:
        en_uzun = u
    collect()
report(en_uzun)
```

## Bölüm 49
```python
toplam = 0
while not rock_ahead(East):
    move(East)
    while dust_here():
        wait()
    toplam += cable_length()
    collect()
report(toplam)
```

## Bölüm 50
```python
toplam = 0
while not rock_ahead(East):
    move(East)
    while dust_here():
        wait()
    toplam += cable_length()
    collect()
move(North)
while not rock_ahead(West):
    move(West)
    while dust_here():
        wait()
    toplam += cable_length()
    collect()
report(toplam)
move(North)
```

## Bölüm 51
```python
move(East)
while dust_here():
    wait()
move(North)
move(East)
move(East)
while dust_here():
    wait()
move(North)
move(North)
move(East)
while dust_here():
    wait()
move(North)
move(East)
move(North)
```

## Bölüm 52
```python
def sabah_turu():
    move(East)
    move(North)
    collect()
sabah_turu()
sabah_turu()
sabah_turu()
```

## Bölüm 53
```python
def raftan_al():
    move(North)
    collect()
    move(South)
move(East)
raftan_al()
move(East)
move(East)
raftan_al()
move(East)
raftan_al()
```

## Bölüm 54
```python
def toz_gec():
    move(North)
    while dust_here():
        wait()
toz_gec()
toz_gec()
collect()
move(East)
move(East)
toz_gec()
toz_gec()
collect()
move(East)
toz_gec()
move(East)
collect()
```

## Bölüm 55
```python
def panel_bak():
    if panel_power() < 50:
        repair()
move(East)
panel_bak()
move(East)
panel_bak()
move(East)
panel_bak()
move(North)
panel_bak()
move(North)
panel_bak()
move(East)
panel_bak()
move(East)
```

## Bölüm 56
```python
def koridor():
    while not rock_ahead(East):
        move(East)
for i in range(3):
    koridor()
    collect()
    move(North)
```

## Bölüm 57
```python
def adim():
    move(East)
    while dust_here():
        wait()
def tur():
    adim()
    adim()
    collect()
    move(North)
tur()
tur()
adim()
move(North)
```

## Bölüm 58
```python
def bak():
    if panel_power() == 0:
        collect()
    elif panel_power() < 50:
        repair()
for i in range(5):
    move(East)
    bak()
for i in range(5):
    move(North)
    bak()
```

## Bölüm 59
```python
def ilerle():
    move(East)
    while dust_here():
        wait()
toplam = 0
for i in range(3):
    ilerle()
    toplam += panel_power()
move(North)
move(North)
for i in range(2):
    ilerle()
    toplam += panel_power()
report(toplam)
move(North)
```

## Bölüm 60
```python
def asagi():
    move(South)
    while dust_here():
        wait()
def doguya():
    move(East)
    if panel_power() < 50:
        repair()
asagi()
asagi()
doguya()
doguya()
asagi()
asagi()
for i in range(3):
    doguya()
asagi()
```
