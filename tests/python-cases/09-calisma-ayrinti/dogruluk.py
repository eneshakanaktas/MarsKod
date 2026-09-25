for x in [0, 1, '', 'a', [], [0], None, 0.0, -1]:
    if x:
        print(repr(x), 'doğru')
    else:
        print(repr(x), 'yanlış')
print(1 and 2, 0 and 2, 1 or 2, 0 or '', not 0, [] or [1])
