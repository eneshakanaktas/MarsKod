def catlak_mi(guc):
    return guc < 50

def aras_mi(satir):
    if "Aras" in satir:
        return True

print(catlak_mi(30), catlak_mi(70))
print(aras_mi("Ece Aras"), aras_mi("Kerem"))
x = aras_mi("Kerem")
print(x is None)
