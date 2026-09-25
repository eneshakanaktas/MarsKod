def say(n):
    if n == 0:
        return 0
    return 1 + say(n - 1)

print(say(900))
