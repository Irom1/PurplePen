import sys

def check_mismatches(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    stack = []
    
    for i, line in enumerate(lines):
        sline = line.strip()
        if sline.startswith('#if'):
            stack.append(('if', i + 1, sline))
        elif sline.startswith('#region'):
            stack.append(('region', i + 1, sline))
        elif sline.startswith('#endregion'):
            if stack and stack[-1][0] == 'region':
                stack.pop()
            else:
                print(f"Mismatched #endregion at line {i+1}: {sline}")
        elif sline.startswith('#endif'):
            if stack and stack[-1][0] == 'if':
                stack.pop()
            else:
                print(f"Mismatched #endif at line {i+1}: {sline}")
                
    for item in stack:
        print(f"Unmatched #{item[0]} at line {item[1]}: {item[2]}")

if __name__ == '__main__':
    check_mismatches(sys.argv[1])
