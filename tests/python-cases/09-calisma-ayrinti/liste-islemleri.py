a = [3, 1, 2]
a.append(5)
a.insert(0, 9)
print(a, a.pop(), a.pop(0), a)
a.remove(1)
print(a, a.index(2), a.count(3))
a.sort()
print(a)
a.reverse()
print(a, a[1:], a[:-1], a[::-1])
b = a
b += [7]
print(a, a is b, len(a))
c = a + [8]
print(c, a, [0] * 3, [1, 2] * 2)
a[0] = 100
a.extend([1, 1])
print(a, a.copy(), sum(a))
a.clear()
print(a)
