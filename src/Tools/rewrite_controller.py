import sys
import re

def rewrite_controller(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    class_match = re.search(r'\bclass\s+Controller\s*(?::[^{]+)?\{', content)
    if not class_match:
        print("Could not find class Controller {")
        sys.exit(1)

    in_string = False
    in_char = False
    in_line_comment = False
    in_block_comment = False
    escape = False
    
    depth = 0
    class_depth = -1 
    
    method_bodies = [] 
    
    i = 0
    n = len(content)
    
    current_method_start = -1
    
    while i < n:
        c = content[i]
        
        if in_line_comment:
            if c == '\n':
                in_line_comment = False
            i += 1
            continue
            
        if in_block_comment:
            if c == '*' and i + 1 < n and content[i+1] == '/':
                in_block_comment = False
                i += 2
            else:
                i += 1
            continue
            
        if in_string:
            if escape:
                escape = False
            elif c == '\\':
                escape = True
            elif c == '"':
                in_string = False
            i += 1
            continue
            
        if in_char:
            if escape:
                escape = False
            elif c == '\\':
                escape = True
            elif c == "'":
                in_char = False
            i += 1
            continue
            
        if c == '/' and i + 1 < n:
            nc = content[i+1]
            if nc == '/':
                in_line_comment = True
                i += 2
                continue
            elif nc == '*':
                in_block_comment = True
                i += 2
                continue
        
        if c == '"':
            in_string = True
            i += 1
            continue
        elif c == "'":
            in_char = True
            i += 1
            continue
            
        if c == '{':
            depth += 1
            if class_depth == -1 and i >= class_match.end() - 1:
                class_depth = depth
                
            elif class_depth != -1 and depth == class_depth + 1:
                j = i - 1
                while j > 0 and content[j] not in ';{}':
                    j -= 1
                decl = content[j+1:i].strip()
                if ')' in decl and '(' in decl:
                    eq_idx = decl.find('=')
                    lparen_idx = decl.find('(')
                    if eq_idx == -1 or lparen_idx < eq_idx:
                        current_method_start = i + 1
            i += 1
            continue
            
        if c == '}':
            if class_depth != -1 and depth == class_depth + 1:
                if current_method_start != -1:
                    method_bodies.append((current_method_start, i))
                    current_method_start = -1
            depth -= 1
            if class_depth != -1 and depth < class_depth:
                class_depth = -1
            i += 1
            continue
            
        i += 1

    new_content = content
    for start, end in reversed(method_bodies):
        original_body = new_content[start:end]
        wrapped = f"\n#if PORTING\nthrow new NotImplementedException(\"Unported Controller method\");\n#else //!PORTING\n{original_body}\n#endif\n"
        new_content = new_content[:start] + wrapped + new_content[end:]
        
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)
        
    print(f"Rewrote {len(method_bodies)} methods.")

if __name__ == '__main__':
    rewrite_controller(sys.argv[1])
