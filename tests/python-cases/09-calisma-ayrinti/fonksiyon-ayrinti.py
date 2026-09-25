def selamla(ad, unvan='Robot'):
    return unvan + ' ' + ad

print(selamla('R1'), selamla('R2', 'Kaşif'), selamla(unvan='Lider', ad='R3'))

def sayac():
    sayi = 0
    def artir():
        nonlocal sayi
        sayi += 1
        return sayi
    return artir

s = sayac()
s()
print(s(), s())

enerji = 10
def harca(miktar):
    global enerji
    enerji -= miktar

harca(3)
print(enerji)

def hicbir_sey():
    return

print(hicbir_sey())
