s = 'MarsKod'
print(s[0], s[-1], s[1:3], s[::-1], s[:4], s[4:], s[::2])
print(s.upper(), s.lower(), '  buz  '.strip(), 'a,b,c'.split(','), 'a b  c'.split())
print('-'.join(['x', 'y', 'z']), s.replace('Kod', 'Koloni'), s.startswith('Mars'), s.endswith('x'))
print(s.find('K'), s.find('z'), 'banana'.count('a'), '42'.isdigit(), 'a1'.isdigit())
print('ab' * 3, 'ab' * 0, 'ab' * -1, 'a' + 'b', 'Ç' < 'Z')
