def f(n):
    if n == 0:
        return 0
    return 1 + f(n - 1)

print(f(998))
print(f(999))
