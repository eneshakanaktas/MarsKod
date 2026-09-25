for i in range(3):
    if i == 5:
        break
else:
    print('bulunamadı')

n = 0
while n < 3:
    n += 1
else:
    print('bitti', n)

for i in range(3):
    if i == 1:
        break
else:
    print('bu yazılmaz')
print(i)
